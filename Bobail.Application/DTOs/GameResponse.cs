using Bobail.Domain.Games;
using Bobail.Domain.Pieces;

namespace Bobail.Application.DTOs;

public class GameResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CurrentTurn { get; set; } = string.Empty;
    public string? Winner { get; set; }

    public bool IsFirstTurn { get; set; }

    public string CurrentPhase { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string? BotColor { get; set; }
    public string? PlayerColor { get; set; }

    public List<PieceDto> Pieces { get; set; } = new();
}
public class PieceDto
{
    public string Type { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
}
