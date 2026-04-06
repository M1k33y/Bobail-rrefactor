namespace Bobail.Domain.Users
{
    public class EmailVerificationToken
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string TokenHash { get; set; } = null!;

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
