namespace Bobail.Application.DTOs;

public class GameReplayResponse
{
    public Guid GameId { get; set; }
    public string OpponentName { get; set; } = string.Empty;
    public string PlayedVs { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public DateTime PlayedAtUtc { get; set; }
    public string? BotDifficulty { get; set; }
    public List<GameReplayStateResponse> States { get; set; } = new();
}

public class GameReplayStateResponse
{
    public int MoveNumber { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CurrentTurn { get; set; } = string.Empty;
    public string? Winner { get; set; }
    public bool IsFirstTurn { get; set; }
    public string CurrentPhase { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string? BotColor { get; set; }
    public List<PieceDto> Pieces { get; set; } = new();
}
