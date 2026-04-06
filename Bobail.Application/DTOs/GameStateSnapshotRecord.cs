namespace Bobail.Application.DTOs;

public class GameStateSnapshotRecord
{
    public int MoveNumber { get; set; }
    public string StateJson { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
