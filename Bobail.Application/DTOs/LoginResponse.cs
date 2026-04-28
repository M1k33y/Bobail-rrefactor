namespace Bobail.Application.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public bool RememberMe { get; set; }
        public string Nickname { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
