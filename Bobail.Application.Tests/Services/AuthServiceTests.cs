using Bobail.Application.Interfaces.Repositories;
using Bobail.Application.Interfaces.Services;
using Bobail.Application.Services;
using Bobail.Application.Validators;
using Bobail.Domain.Common;
using Bobail.Domain.Users;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Bobail.Application.Tests.Services
{
    public class AuthServiceTests
    {
        private static AuthService CreateService(
            Mock<IUserRepository> userRepoMock,
            Mock<IEmailVerificationTokenRepository>? verificationTokenRepoMock = null,
            Mock<IPasswordResetTokenRepository>? passwordResetTokenRepoMock = null,
            Mock<IEmailSender>? emailSenderMock = null,
            IConfiguration? config = null)
        {
            var registerValidatorMock = new Mock<IValidator<(string, string, string)>>();
            registerValidatorMock
                .Setup(x => x.Validate(It.IsAny<(string, string, string)>()))
                .Returns(new ValidationResult());

            var loginValidatorMock = new Mock<IValidator<(string, string)>>();
            loginValidatorMock
                .Setup(x => x.Validate(It.IsAny<(string, string)>()))
                .Returns(new ValidationResult());

            return new AuthService(
                userRepoMock.Object,
                (verificationTokenRepoMock ?? new Mock<IEmailVerificationTokenRepository>()).Object,
                (passwordResetTokenRepoMock ?? new Mock<IPasswordResetTokenRepository>()).Object,
                (emailSenderMock ?? new Mock<IEmailSender>()).Object,
                config ?? GetConfig(),
                registerValidatorMock.Object,
                loginValidatorMock.Object);
        }

        private static IConfiguration GetConfig()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["Jwt:Key"])
                .Returns("SUPER_SECRET_TEST_KEY_123456789_ABC");
            configMock.Setup(x => x["Frontend:BaseUrl"])
                .Returns("http://localhost:5173");

            return configMock.Object;
        }

        [Fact]
        public async Task Register_Should_Create_Unverified_User_And_Send_Email()
        {
            var userRepoMock = new Mock<IUserRepository>();
            var verificationTokenRepoMock = new Mock<IEmailVerificationTokenRepository>();
            var emailSenderMock = new Mock<IEmailSender>();
            User? capturedUser = null;

            userRepoMock.Setup(x => x.AddAsync(It.IsAny<User>()))
                .Callback<User>(user => capturedUser = user);

            var service = CreateService(
                userRepoMock,
                verificationTokenRepoMock: verificationTokenRepoMock,
                emailSenderMock: emailSenderMock);

            var response = await service.RegisterAsync("test@mail.com", "StrongPass1", "mihai");

            Assert.NotEqual(Guid.Empty, response.UserId);
            Assert.NotNull(capturedUser);
            Assert.True(capturedUser!.IsActive);
            Assert.False(capturedUser!.IsEmailVerified);
            Assert.True(BCrypt.Net.BCrypt.Verify("StrongPass1", capturedUser.PasswordHash));
            verificationTokenRepoMock.Verify(x => x.AddAsync(It.IsAny<EmailVerificationToken>()), Times.Once);
            emailSenderMock.Verify(x => x.SendAsync("test@mail.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Login_Should_Fail_When_User_Is_Inactive()
        {
            var password = "StrongPass1";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@mail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsActive = false,
                IsEmailVerified = true
            };

            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            var service = CreateService(userRepoMock);

            var exception = await Assert.ThrowsAsync<DomainException>(() =>
                service.LoginAsync("test@mail.com", password, false));

            Assert.Equal("This user is currently banned.", exception.Message);
        }

        [Fact]
        public async Task Login_Should_Fail_When_Email_Is_Not_Verified()
        {
            var password = "StrongPass1";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@mail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsEmailVerified = false
            };

            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            var service = CreateService(userRepoMock);

            var exception = await Assert.ThrowsAsync<DomainException>(() =>
                service.LoginAsync("test@mail.com", password, false));

            Assert.Equal("Please verify your email before logging in", exception.Message);
        }

        [Fact]
        public async Task Login_Should_Return_Jwt_When_Credentials_Are_Valid_And_Email_Is_Verified()
        {
            var password = "StrongPass1";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@mail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = 1,
                IsActive = true,
                IsEmailVerified = true
            };

            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            var service = CreateService(userRepoMock);

            var response = await service.LoginAsync("test@mail.com", password, true);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(response.Token);

            Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Email && c.Value == user.Email);
            Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == user.Role.ToString());
            Assert.True(response.RememberMe);
            Assert.Equal(user.Id, response.UserId);
            Assert.Equal("Admin", response.Role);
        }

        [Fact]
        public async Task ForgotPassword_Should_Send_Email_For_Verified_User()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@mail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("StrongPass1"),
                Nickname = "mihai",
                IsActive = true,
                IsEmailVerified = true
            };

            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(x => x.GetByEmailAsync("test@mail.com"))
                .ReturnsAsync(user);

            var passwordResetTokenRepoMock = new Mock<IPasswordResetTokenRepository>();
            var emailSenderMock = new Mock<IEmailSender>();

            var service = CreateService(
                userRepoMock,
                passwordResetTokenRepoMock: passwordResetTokenRepoMock,
                emailSenderMock: emailSenderMock);

            var response = await service.RequestPasswordResetAsync("test@mail.com");

            Assert.Equal("If the account exists, a password reset email has been sent.", response.Message);
            passwordResetTokenRepoMock.Verify(x => x.AddAsync(It.IsAny<PasswordResetToken>()), Times.Once);
            emailSenderMock.Verify(x => x.SendAsync("test@mail.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ResetPassword_Should_Update_Password_And_Consume_Token()
        {
            var rawToken = "plain-reset-token";
            var hashedToken = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(rawToken)));

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@mail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpass"),
                Nickname = "mihai",
                IsActive = true,
                IsEmailVerified = true
            };

            var resetToken = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = hashedToken,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
                Used = false
            };

            var userRepoMock = new Mock<IUserRepository>();
            var passwordResetTokenRepoMock = new Mock<IPasswordResetTokenRepository>();
            User? updatedUser = null;

            passwordResetTokenRepoMock.Setup(x => x.GetByTokenHashAsync(hashedToken))
                .ReturnsAsync(resetToken);
            userRepoMock.Setup(x => x.GetByIdAsync(user.Id))
                .ReturnsAsync(user);
            userRepoMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Callback<User>(candidate => updatedUser = candidate);

            var service = CreateService(
                userRepoMock,
                passwordResetTokenRepoMock: passwordResetTokenRepoMock);

            await service.ResetPasswordAsync(rawToken, "Newpassword1");

            Assert.NotNull(updatedUser);
            Assert.True(BCrypt.Net.BCrypt.Verify("Newpassword1", updatedUser!.PasswordHash));
            passwordResetTokenRepoMock.Verify(x => x.MarkAsUsedAsync(resetToken.Id), Times.Once);
            passwordResetTokenRepoMock.Verify(x => x.DeleteByUserIdAsync(user.Id), Times.Once);
        }

        [Fact]
        public async Task ResetPassword_Should_Fail_When_NewPassword_Does_Not_Meet_Policy()
        {
            var userRepoMock = new Mock<IUserRepository>();
            var passwordResetTokenRepoMock = new Mock<IPasswordResetTokenRepository>();
            var service = CreateService(
                userRepoMock,
                passwordResetTokenRepoMock: passwordResetTokenRepoMock);

            var exception = await Assert.ThrowsAsync<DomainException>(() =>
                service.ResetPasswordAsync("token", "weakpass"));

            Assert.Equal(PasswordPolicy.PasswordRequirementsMessage, exception.Message);
        }

        [Fact]
        public async Task VerifyEmail_Should_Mark_User_As_Verified_And_Delete_Tokens()
        {
            var rawToken = "plain-verify-token";
            var hashedToken = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(rawToken)));

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@mail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpass"),
                Nickname = "mihai",
                IsActive = true,
                IsEmailVerified = false
            };

            var verificationToken = new EmailVerificationToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = hashedToken,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
            };

            var userRepoMock = new Mock<IUserRepository>();
            var verificationTokenRepoMock = new Mock<IEmailVerificationTokenRepository>();
            User? updatedUser = null;

            verificationTokenRepoMock.Setup(x => x.GetByTokenHashAsync(hashedToken))
                .ReturnsAsync(verificationToken);
            userRepoMock.Setup(x => x.GetByIdAsync(user.Id))
                .ReturnsAsync(user);
            userRepoMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Callback<User>(candidate => updatedUser = candidate);

            var service = CreateService(
                userRepoMock,
                verificationTokenRepoMock: verificationTokenRepoMock);

            await service.VerifyEmailAsync(rawToken);

            Assert.NotNull(updatedUser);
            Assert.True(updatedUser!.IsEmailVerified);
            Assert.NotNull(updatedUser.EmailVerifiedAtUtc);
            verificationTokenRepoMock.Verify(x => x.DeleteByUserIdAsync(user.Id), Times.Once);
        }
    }
}
