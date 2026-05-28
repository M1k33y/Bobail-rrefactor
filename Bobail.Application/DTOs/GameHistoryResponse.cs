namespace Bobail.Application.DTOs;

public class GameHistoryResponse
{
    public Guid GameId { get; set; }
    public string OpponentName { get; set; } = string.Empty;
    public string PlayedVs { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? EndReason { get; set; }
    public DateTime PlayedAtUtc { get; set; }
    public string Mode { get; set; } = string.Empty;
    public string? BotDifficulty { get; set; }
}
