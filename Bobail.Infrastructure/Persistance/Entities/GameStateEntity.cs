namespace Bobail.Infrastructure.Persistance.Entities;

public class GameStateEntity
{
    public Guid Id { get; set; }

    public Guid GameId { get; set; }
    public GameEntity Game { get; set; } = null!;

    public int MoveNumber { get; set; }

    public string StateJson { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
