using Bobail.Domain.Games;
using Bobail.Domain.Pieces;

namespace Bobail.API.DTOs;

public class GameResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CurrentTurn { get; set; } = string.Empty;
    public string? Winner { get; set; }

    public List<PieceDto> Pieces { get; set; } = new();

    public bool IsFirstTurn { get; set; }
}

public class PieceDto
{
    public string Type { get; set; } = string.Empty;
    public int Row { get; set; }
    public int Column { get; set; }
}
