using Bobail.Application.DTOs;

namespace Bobail.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterAsync(string email, string password, string nickname);
        Task<LoginResponse> LoginAsync(string email, string password, bool rememberMe);
        Task<ForgotPasswordResponse> RequestPasswordResetAsync(string email);
        Task ResetPasswordAsync(string token, string newPassword);
        Task VerifyEmailAsync(string token);
        Task ResendVerificationEmailAsync(string email);
    }
}
