using Bobail.Domain.Games;

namespace Bobail.Application.DTOs;

public static class GameResponseMapper
{
    public static GameResponse ToResponse(Game game, PlayerColor? playerColor = null)
    {
        return new GameResponse
        {
            Id = game.Id,
            Status = game.Status.ToString(),
            CurrentTurn = game.CurrentTurn.ToString(),
            Winner = game.Winner?.ToString(),
            IsFirstTurn = game.IsFirstTurn,
            CurrentPhase = game.CurrentPhase.ToString(),
            Mode = game.Mode.ToString(),
            BotColor = game.BotColor?.ToString(),
            PlayerColor = playerColor?.ToString(),
            Pieces = game.Board.Pieces.Select(p => new PieceDto
            {
                Type = p.Type.ToString(),
                Owner = p.Owner?.ToString(),
                Row = p.Position.Row,
                Column = p.Position.Column
            }).ToList()
        };
    }
}
