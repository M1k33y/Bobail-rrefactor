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
        public async Task Register_Should_Return_Ok_And_Send_Verification_Email()
        {
            var email = $"test_{Guid.NewGuid()}@mail.com";

            var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
            {
                Email = email,
                Password = ValidPassword,
                Nickname = "mihai"
            });

            var body = await response.Content.ReadFromJsonAsync<RegisterResponse>();
            var emails = GetEmails();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            Assert.Equal("Account created. Please check your email to verify your account.", body!.Message);
            Assert.Contains(emails, message => message.ToEmail == email && message.Subject.Contains("Verify"));
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
        public async Task VerifyEmail_Should_Allow_Login()
        {
            var email = $"test_{Guid.NewGuid()}@mail.com";

            await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
            {
                Email = email,
                Password = ValidPassword,
                Nickname = "mihai"
            });

            var token = ExtractLastTokenForEmail(email, "/verify-email?token=");

            var verifyResponse = await _client.PostAsJsonAsync("/api/auth/verify-email", new VerifyEmailRequest
            {
                Token = token
            });

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Email = email,
                Password = ValidPassword,
                RememberMe = true
            });

            var body = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

            Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            Assert.NotNull(body);
            Assert.False(string.IsNullOrWhiteSpace(body!.Token));
            Assert.True(body.RememberMe);
        }

        [Fact]
        public async Task Login_Should_Fail_With_Wrong_Password()
        {
            var email = $"test_{Guid.NewGuid()}@mail.com";

            await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
            {
                Email = email,
                Password = ValidPassword,
                Nickname = "mihai"
            });

            var token = ExtractLastTokenForEmail(email, "/verify-email?token=");
            await _client.PostAsJsonAsync("/api/auth/verify-email", new VerifyEmailRequest
            {
                Token = token
            });

            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Email = email,
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
                Password = ValidPassword
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_Should_Return_Valid_Jwt()
        {
            var email = $"test_{Guid.NewGuid()}@mail.com";

            await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
            {
                Email = email,
                Password = ValidPassword,
                Nickname = "mihai"
            });

            var token = ExtractLastTokenForEmail(email, "/verify-email?token=");
            await _client.PostAsJsonAsync("/api/auth/verify-email", new VerifyEmailRequest
            {
                Token = token
            });

            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Email = email,
                Password = ValidPassword
            });

            var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(body!.Token);

            Assert.NotNull(jwt);
            Assert.Contains(jwt.Claims, c => c.Type.Contains("email"));
        }

        [Fact]
        public async Task ForgotPassword_Should_Send_Reset_Email_For_Verified_User()
        {
            var email = $"test_{Guid.NewGuid()}@mail.com";

            await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
            {
                Email = email,
                Password = ValidPassword,
                Nickname = "mihai"
            });

            var verificationToken = ExtractLastTokenForEmail(email, "/verify-email?token=");
            await _client.PostAsJsonAsync("/api/auth/verify-email", new VerifyEmailRequest
            {
                Token = verificationToken
            });

            var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", new ForgotPasswordRequest
            {
                Email = email
            });

            var body = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
            var emails = GetEmails();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            Assert.Equal("If the account exists, a password reset email has been sent.", body!.Message);
            Assert.Contains(emails, message => message.ToEmail == email && message.Subject.Contains("Reset"));
        }

        [Fact]
        public async Task ResetPassword_Should_Allow_Login_With_New_Password()
        {
            var email = $"test_{Guid.NewGuid()}@mail.com";

            await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
            {
                Email = email,
                Password = ValidPassword,
                Nickname = "mihai"
            });

            var verificationToken = ExtractLastTokenForEmail(email, "/verify-email?token=");
            await _client.PostAsJsonAsync("/api/auth/verify-email", new VerifyEmailRequest
            {
                Token = verificationToken
            });

            await _client.PostAsJsonAsync("/api/auth/forgot-password", new ForgotPasswordRequest
            {
                Email = email
            });

            var resetToken = ExtractLastTokenForEmail(email, "/reset-password?token=");

            var resetResponse = await _client.PostAsJsonAsync("/api/auth/reset-password", new ResetPasswordRequest
            {
                Token = resetToken,
                NewPassword = ValidResetPassword
            });

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Email = email,
                Password = ValidResetPassword
            });

            Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
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
