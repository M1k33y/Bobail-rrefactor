namespace Bobail.Infrastructure.Persistance.Entities;

public class GameEntity
{
    public Guid Id { get; set; }

    public string StateJson { get; set; } = null!;

    public int Status { get; set; }
    public int CurrentTurn { get; set; }

    public int Mode { get; set; }
    public int? BotDifficulty { get; set; }

    public Guid? WinnerUserId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

}