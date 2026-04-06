namespace Bobail.Infrastructure.Persistance.Entities;

public class PasswordResetTokenEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTime ExpiresAtUtc { get; set; }

    public bool Used { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UsedAtUtc { get; set; }

    public UserEntity User { get; set; } = null!;
}
