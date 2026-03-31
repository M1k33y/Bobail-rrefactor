using BCrypt.Net;
using Bobail.Application.Interfaces.Repositories;
using Bobail.Application.Interfaces.Services;
using Bobail.Domain.Users;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;
    private readonly IValidator<(string Email, string Password, string Nickname)> _registerValidator;
    private readonly IValidator<(string Email, string Password)> _loginValidator;

    public AuthService(
      IUserRepository userRepository,
      IConfiguration config,
      IValidator<(string Email, string Password, string Nickname)> registerValidator,
      IValidator<(string Email, string Password)> loginValidator)
    {
        _userRepository = userRepository;
        _config = config;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<Guid> RegisterAsync(string email, string password, string nickname)
    {

        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing != null)
            throw new Exception("Email already exists");

        var result = _registerValidator.Validate((email, password, nickname));

        if (!result.IsValid)
            throw new Exception(result.Errors.First().ErrorMessage);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = 0,
            CreatedAt = DateTime.UtcNow,
            Nickname = nickname
        };

        await _userRepository.AddAsync(user);

        return user.Id;
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var result = _loginValidator.Validate((email, password));

        if (!result.IsValid)
            throw new Exception(result.Errors.First().ErrorMessage);

        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new Exception("Invalid credentials");

        return GenerateJwt(user);
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(3),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}