namespace Bobail.Infrastructure.Persistance.Entities;

public class GamePlayerEntity
{
    public Guid Id { get; set; }

    public Guid GameId { get; set; }

    public GameEntity Game { get; set; } = null!;

    public Guid? UserId { get; set; } // null pentru bot

    public int Color { get; set; } // 0 / 1

    public bool IsBot { get; set; }
}
