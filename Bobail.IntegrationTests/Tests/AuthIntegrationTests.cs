using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Bobail.Application.DTOs;
using Bobail.Infrastructure.Email;
using Bobail.IntegrationTests.Factories;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bobail.IntegrationTests
{
    public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private const string ValidPassword = "StrongPass1";
        private const string ValidResetPassword = "Newpassword1";

        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public AuthIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
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

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_Should_Fail_When_Email_Already_Exists()
        {
            var email = $"test_{Guid.NewGuid()}@mail.com";
            var user = new RegisterRequest
            {
                Email = email,
                Password = ValidPassword,
                Nickname = "mihai"
            };

            await _client.PostAsJsonAsync("/api/auth/register", user);

            var response = await _client.PostAsJsonAsync("/api/auth/register", user);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_Should_Fail_When_Email_Is_Not_Verified()
        {
            var email = $"test_{Guid.NewGuid()}@mail.com";

            await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
            {
                Email = email,
                Password = ValidPassword,
                Nickname = "mihai"
            });

            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Email = email,
                Password = ValidPassword,
                RememberMe = true
            });

            var error = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("verify your email", error, StringComparison.OrdinalIgnoreCase);
        }

       

        
        [Fact]
        public async Task Login_Should_Fail_When_User_Not_Found()
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Email = "notfound@mail.com",
                Password = ValidPassword
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

       

      

       
        private IReadOnlyList<SentEmailMessage> GetEmails()
        {
            using var scope = _factory.Services.CreateScope();
            var outbox = scope.ServiceProvider.GetRequiredService<IEmailOutbox>();
            return outbox.Messages;
        }

        private string ExtractLastTokenForEmail(string email, string pathSegment)
        {
            var message = GetEmails()
                .Last(x => x.ToEmail == email && x.HtmlBody.Contains(pathSegment, StringComparison.OrdinalIgnoreCase));

            var match = Regex.Match(message.HtmlBody, @"token=([^""&<]+)");
            Assert.True(match.Success, $"No token found in email body: {message.HtmlBody}");
            return Uri.UnescapeDataString(match.Groups[1].Value);
        }
    }
}
