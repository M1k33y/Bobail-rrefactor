namespace Bobail.Infrastructure.Persistance.Entities;

public class UserEntity
{
    public Guid Id { get; set; }

    public string Nickname { get; set; } = null!;

    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    public int Role { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsEmailVerified { get; set; }

    public DateTime? EmailVerifiedAtUtc { get; set; }
}
