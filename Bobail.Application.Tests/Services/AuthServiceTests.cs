using Bobail.Application.Interfaces.Repositories;
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
        private AuthService CreateService(Mock<IUserRepository> repoMock, IConfiguration config)
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
                repoMock.Object,
                config,
                registerValidatorMock.Object,
                loginValidatorMock.Object);
        }

        private IConfiguration GetConfig()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["Jwt:Key"])
                      .Returns("SUPER_SECRET_TEST_KEY_123456789_ABC");

            return configMock.Object;
        }

        [Fact]
        public async Task Register_Should_Create_User()
        {
            var repoMock = new Mock<IUserRepository>();

            var service = CreateService(repoMock, GetConfig());

            var userId = await service.RegisterAsync("test@mail.com", "123456", "mihai");

            repoMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Register_Should_Hash_Password()
        {
            var repoMock = new Mock<IUserRepository>();

            User capturedUser = null;

            repoMock.Setup(x => x.AddAsync(It.IsAny<User>()))
                    .Callback<User>(u => capturedUser = u);

            var service = CreateService(repoMock, GetConfig());

            await service.RegisterAsync("test@mail.com", "123456", "mihai");

            Assert.NotNull(capturedUser);
            Assert.NotEqual("123456", capturedUser.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify("123456", capturedUser.PasswordHash));
        }

        [Fact]
        public async Task Login_Should_Fail_When_Password_Is_Wrong()
        {
            var user = new User
            {
                Email = "test@mail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct")
            };

            var repoMock = new Mock<IUserRepository>();
            repoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync(user);

            var service = CreateService(repoMock, GetConfig());

            await Assert.ThrowsAsync<Exception>(() =>
                service.LoginAsync("test@mail.com", "wrong"));
        }

        [Fact]
        public async Task Login_Should_Fail_When_User_Not_Found()
        {
            var repoMock = new Mock<IUserRepository>();
            repoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((User?)null);

            var service = CreateService(repoMock, GetConfig());

            await Assert.ThrowsAsync<Exception>(() =>
                service.LoginAsync("test@mail.com", "123456"));
        }

        [Fact]
        public async Task Login_Should_Return_Jwt_When_Credentials_Valid()
        {
            var password = "123456";

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@mail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = 0
            };

            var repoMock = new Mock<IUserRepository>();
            repoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync(user);

            var service = CreateService(repoMock, GetConfig());

            var token = await service.LoginAsync("test@mail.com", password);

            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task Login_Should_Return_Jwt_With_Correct_Claims()
        {
            var password = "123456";

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@mail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = 1
            };

            var repoMock = new Mock<IUserRepository>();
            repoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync(user);

            var service = CreateService(repoMock, GetConfig());

            var token = await service.LoginAsync("test@mail.com", password);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Email && c.Value == user.Email);
            Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == user.Role.ToString());
        }
    }
}