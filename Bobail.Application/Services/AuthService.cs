using BCrypt.Net;
using Bobail.Application.DTOs;
using Bobail.Application.Interfaces.Repositories;
using Bobail.Application.Interfaces.Services;
using Bobail.Application.Validators;
using Bobail.Domain.Common;
using Bobail.Domain.Users;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Bobail.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _config;
    private readonly IValidator<(string Email, string Password, string Nickname)> _registerValidator;
    private readonly IValidator<(string Email, string Password)> _loginValidator;

    public AuthService(
        IUserRepository userRepository,
        IEmailVerificationTokenRepository emailVerificationTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IEmailSender emailSender,
        IConfiguration config,
        IValidator<(string Email, string Password, string Nickname)> registerValidator,
        IValidator<(string Email, string Password)> loginValidator)
    {
        _userRepository = userRepository;
        _emailVerificationTokenRepository = emailVerificationTokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailSender = emailSender;
        _config = config;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<RegisterResponse> RegisterAsync(string email, string password, string nickname)
    {
        email = NormalizeEmail(email);
        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing != null)
            throw new DomainException("Email already exists");

        var result = _registerValidator.Validate((email, password, nickname));
        if (!result.IsValid)
            throw new DomainException(result.Errors.First().ErrorMessage);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = 0,
            CreatedAt = DateTime.UtcNow,
            Nickname = nickname,
            IsActive = true,
            IsEmailVerified = false
        };

        await _userRepository.AddAsync(user);
        await SendVerificationEmailAsync(user);

        return new RegisterResponse
        {
            UserId = user.Id,
            Message = "Account created. Please check your email to verify your account."
        };
    }

    public async Task<LoginResponse> LoginAsync(string email, string password, bool rememberMe)
    {
        email = NormalizeEmail(email);
        var result = _loginValidator.Validate((email, password));
        if (!result.IsValid)
            throw new DomainException(result.Errors.First().ErrorMessage);

        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new DomainException("Invalid credentials");

        if (!user.IsActive)
            throw new DomainException("This user is currently banned.");

        if (!user.IsEmailVerified)
            throw new DomainException("Please verify your email before logging in");

        return GenerateJwt(user, rememberMe);
    }

    public async Task<ForgotPasswordResponse> RequestPasswordResetAsync(string email)
    {
        email = NormalizeEmail(email);

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email required");

        if (!new EmailAddressAttribute().IsValid(email))
            throw new DomainException("Invalid email");

        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !user.IsActive || !user.IsEmailVerified)
        {
            return new ForgotPasswordResponse
            {
                Message = "If the account exists, a password reset email has been sent."
            };
        }

        var rawToken = CreateRawToken();
        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = HashToken(rawToken),
            ExpiresAtUtc = DateTime.UtcNow.AddHours(1),
            CreatedAtUtc = DateTime.UtcNow,
            Used = false
        };

        await _passwordResetTokenRepository.DeleteByUserIdAsync(user.Id);
        await _passwordResetTokenRepository.AddAsync(resetToken);

        var resetUrl = BuildFrontendUrl("/reset-password", rawToken);
        await _emailSender.SendAsync(
            user.Email,
            "Reset your Bobail password",
            $"<p>Hi {user.Nickname},</p><p>Click <a href=\"{resetUrl}\">here</a> to reset your password.</p><p>This link expires in 1 hour.</p>");

        return new ForgotPasswordResponse
        {
            Message = "If the account exists, a password reset email has been sent."
        };
    }

    public async Task ResetPasswordAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException("Reset token required");

        ValidatePassword(newPassword);

        var tokenHash = HashToken(token.Trim());
        var resetToken = await _passwordResetTokenRepository.GetByTokenHashAsync(tokenHash);

        if (resetToken == null || resetToken.Used || resetToken.ExpiresAtUtc < DateTime.UtcNow)
            throw new DomainException("Reset token is invalid or expired");

        var user = await _userRepository.GetByIdAsync(resetToken.UserId);
        if (user == null)
            throw new DomainException("Reset token is invalid or expired");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        await _userRepository.UpdateAsync(user);
        await _passwordResetTokenRepository.MarkAsUsedAsync(resetToken.Id);
        await _passwordResetTokenRepository.DeleteByUserIdAsync(user.Id);
    }

    public async Task VerifyEmailAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException("Verification token required");

        var tokenHash = HashToken(token.Trim());
        var verificationToken = await _emailVerificationTokenRepository.GetByTokenHashAsync(tokenHash);

        if (verificationToken == null || verificationToken.ExpiresAtUtc < DateTime.UtcNow)
            throw new DomainException("Verification token is invalid or expired");

        var user = await _userRepository.GetByIdAsync(verificationToken.UserId);
        if (user == null)
            throw new DomainException("Verification token is invalid or expired");

        user.IsEmailVerified = true;
        user.EmailVerifiedAtUtc = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _emailVerificationTokenRepository.DeleteByUserIdAsync(user.Id);
    }

    public async Task ResendVerificationEmailAsync(string email)
    {
        email = NormalizeEmail(email);

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email required");

        if (!new EmailAddressAttribute().IsValid(email))
            throw new DomainException("Invalid email");

        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !user.IsActive || user.IsEmailVerified)
            return;

        await SendVerificationEmailAsync(user);
    }

    private LoginResponse GenerateJwt(User user, bool rememberMe)
    {
        var expiresAtUtc = rememberMe
            ? DateTime.UtcNow.AddDays(30)
            : DateTime.UtcNow.AddHours(3);
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: creds
        );

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAtUtc,
            RememberMe = rememberMe,
            Nickname = user.Nickname,
            UserId = user.Id,
            Role = ToRoleName(user.Role)
        };
    }

    private static string ToRoleName(int role)
    {
        return role == 1 ? "Admin" : "User";
    }

    private async Task SendVerificationEmailAsync(User user)
    {
        var rawToken = CreateRawToken();
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = HashToken(rawToken),
            ExpiresAtUtc = DateTime.UtcNow.AddHours(24),
            CreatedAtUtc = DateTime.UtcNow
        };

        await _emailVerificationTokenRepository.DeleteByUserIdAsync(user.Id);
        await _emailVerificationTokenRepository.AddAsync(verificationToken);

        var verificationUrl = BuildFrontendUrl("/verify-email", rawToken);
        await _emailSender.SendAsync(
            user.Email,
            "Verify your Bobail account",
            $"<p>Hi {user.Nickname},</p><p>Click <a href=\"{verificationUrl}\">here</a> to verify your email address.</p><p>This link expires in 24 hours.</p>");
    }

    private string BuildFrontendUrl(string path, string rawToken)
    {
        var frontendBaseUrl = _config["Frontend:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5173";
        return $"{frontendBaseUrl}{path}?token={Uri.EscapeDataString(rawToken)}";
    }

    private static string CreateRawToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    }

    private static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new DomainException("Password required");

        if (!PasswordPolicy.IsValid(password))
            throw new DomainException(PasswordPolicy.PasswordRequirementsMessage);
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email required");

        return email.Trim().ToLowerInvariant();
    }
}
