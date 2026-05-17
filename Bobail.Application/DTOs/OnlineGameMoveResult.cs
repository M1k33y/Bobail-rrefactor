namespace Bobail.Application.DTOs;

public class OnlineGameMoveResult
{
    public string MoveType { get; set; } = string.Empty;
    public string PlayerColor { get; set; } = string.Empty;
    public GameResponse Game { get; set; } = new();
}
