using Bobail.Application.Interfaces.Services;

using Bobail.Domain.Games;
using Bobail.Domain.Board;
using Bobail.Domain.Pieces;

namespace Bobail.Infrastructure.Bots;

public class HardBoardEvaluator : IBoardEvaluator
{
    private const int WinScore = 1_000_000;
    private const int LossScore = -1_000_000;

    public int Evaluate(Game game, PlayerColor botColor)
    {
        if (game.Status == GameStatus.Finished)
        {
            return game.Winner == botColor
                ? WinScore
                : LossScore;
        }

        var opponent = Opponent(botColor);

        int score = 0;

        score += EvaluateProgress(game, botColor);
        score -= EvaluateProgress(game, opponent);

        score += EvaluateProtection(game, botColor);
        score -= EvaluateProtection(game, opponent);

        score += EvaluateCenterControl(game, botColor);
        score += EvaluateMobilityControl(game, botColor);
        score += EvaluateCorridor(game, botColor);

        return score;
    }

    private PlayerColor Opponent(PlayerColor c)
        => c == PlayerColor.Red
            ? PlayerColor.Green
            : PlayerColor.Red;

   
    private int EvaluateProgress(Game game, PlayerColor color)
    {
        var bobail = game.Board.Pieces.First(p => p.IsBobail);

        int boardSize = 5;
        int targetRow = color == PlayerColor.Red ? 0 : boardSize - 1;

        int distance = Math.Abs(bobail.Position.Row - targetRow);
        int maxDistance = boardSize - 1;

        int progress = maxDistance - distance;

        
        return progress * progress * 400;
    }

  
    private int EvaluateProtection(Game game, PlayerColor color)
    {
        var bobail = game.Board.Pieces.First(p => p.IsBobail);
        int score = 0;

        var neighbors = Adjacent(bobail.Position);

        foreach (var pos in neighbors)
        {
            var piece = game.Board.GetPieceAt(pos);

            if (piece != null && piece.Owner == color)
                score += 120;

            if (piece != null && piece.Owner == Opponent(color))
                score -= 100;
        }

        return score;
    }

    private int EvaluateCenterControl(Game game, PlayerColor color)
    {
        int score = 0;

        foreach (var piece in game.Board.Pieces)
        {
            if (!piece.IsBobail && piece.Owner == color)
            {
                if (piece.Position.Row == 2)
                    score += 100;
            }
        }

        return score;
    }


    private int EvaluateMobilityControl(Game game, PlayerColor color)
    {
        var moves = game.GetValidBobailMoves();

        return (8 - moves.Count) * 80;
    }

  
    private int EvaluateCorridor(Game game, PlayerColor color)
    {
        var bobail = game.Board.Pieces.First(p => p.IsBobail);

        int score = 0;

        int direction = color == PlayerColor.Red ? -1 : 1;

        int nextRow = bobail.Position.Row + direction;

        if (nextRow >= 0 && nextRow < 5)
        {
            var forwardPos = new Position(nextRow, bobail.Position.Column);

            if (game.Board.IsEmpty(forwardPos))
                score += 200; // drum liber inainte
        }

        return score;
    }

    private List<Position> Adjacent(Position pos)
    {
        var result = new List<Position>();

        if (pos.Row - 1 >= 0)
            result.Add(new Position(pos.Row - 1, pos.Column));

        if (pos.Row + 1 < 5)
            result.Add(new Position(pos.Row + 1, pos.Column));

        if (pos.Column - 1 >= 0)
            result.Add(new Position(pos.Row, pos.Column - 1));

        if (pos.Column + 1 < 5)
            result.Add(new Position(pos.Row, pos.Column + 1));

        return result;
    }
}