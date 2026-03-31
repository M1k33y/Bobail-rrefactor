using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Bobail.IntegrationTests.Factories;
using Bobail.Application.DTOs;

namespace Bobail.IntegrationTests
{
    public class AuthIntegrationTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_Should_Return_Ok()
        {
            

            var email = $"test_{Guid.NewGuid()}@mail.com"; 

            var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
            {
                Email = email,
                Password = "123456",
                Nickname = "mihai"
            });

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Register_Should_Fail_When_Invalid_Data()
        {
            var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
            {
                Email = "invalid",
                Password = "123",
                Nickname = "mi"
            });

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_Should_Fail_When_Email_Already_Exists()
        {
            var user = new RegisterRequest
            {
                Email = "test@mail.com",
                Password = "123456",
                Nickname = "mihai"
            };

            await _client.PostAsJsonAsync("/api/auth/register", user);

            var response = await _client.PostAsJsonAsync("/api/auth/register", user);

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_Should_Return_Token()
        {
            var user = new RegisterRequest
            {
                Email = "test@mail.com",
                Password = "123456",
                Nickname = "mihai"
            };

            await _client.PostAsJsonAsync("/api/auth/register", user);

            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Email = "test@mail.com",
                Password = "123456"
            });

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);

            var token = content.Trim('"');

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(string.IsNullOrEmpty(token));
        }

        [Fact]
        public async Task Login_Should_Fail_With_Wrong_Password()
        {
            await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
            {
                Email = "test@mail.com",
                Password = "123456",
                Nickname = "mihai"
            });

            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Email = "test@mail.com",
                Password = "wrong"
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_Should_Fail_When_User_Not_Found()
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Email = "notfound@mail.com",
                Password = "123456"
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_Should_Return_Valid_Jwt()
        {
            await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
            {
                Email = "test@mail.com",
                Password = "123456",
                Nickname = "mihai"
            });

            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Email = "test@mail.com",
                Password = "123456"
            });

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);

            var token = content.Trim('"'); // 🔥 IMPORTANT

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.NotNull(jwt);
            Assert.Contains(jwt.Claims, c => c.Type.Contains("email"));
        }
    }
}