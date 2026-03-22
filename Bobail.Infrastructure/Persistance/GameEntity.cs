namespace Bobail.Infrastructure.Persistence;

public class GameEntity
{
    public Guid Id { get; set; }

    public string StateJson { get; set; } = null!;
}