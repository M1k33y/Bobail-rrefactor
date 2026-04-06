namespace Bobail.Application.DTOs
{
    public class RegisterResponse
    {
        public Guid UserId { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
