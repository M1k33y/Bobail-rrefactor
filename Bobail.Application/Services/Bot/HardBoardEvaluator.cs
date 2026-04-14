using Bobail.Application.Interfaces.Services;
using Bobail.Application.Services.Bot;
using Bobail.Domain.Board;
using Bobail.Domain.Games;
using Bobail.Domain.Pieces;

namespace Bobail.Infrastructure.Bots;

public class HardBoardEvaluator : IBoardEvaluator
{
    private static readonly Direction[] AllDirections =
    {
        new(-1, 0),
        new(1, 0),
        new(0, -1),
        new(0, 1),
        new(-1, -1),
        new(-1, 1),
        new(1, -1),
        new(1, 1)
    };

    private const int WinScore = 1_000_000;
    private const int LossScore = -1_000_000;
    private readonly EvaluationWeights _weights;

    public HardBoardEvaluator(EvaluationWeights weights)
    {
        _weights = weights;
    }

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

        score += EvaluateEndgamePressure(game, botColor);
        score -= EvaluateEndgamePressure(game, opponent);

        score += EvaluateAdjacencyControl(game, botColor);
        score -= EvaluateAdjacencyControl(game, opponent);

        score += EvaluateCenterControl(game, botColor);
        score -= EvaluateCenterControl(game, opponent);

        score += EvaluateForwardMobility(game, botColor);
        score -= EvaluateForwardMobility(game, opponent);

        score += EvaluateCorridor(game, botColor);
        score -= EvaluateCorridor(game, opponent);

        score += EvaluateImmediateWinThreat(game, botColor);
        score -= EvaluateImmediateWinThreat(game, opponent);

        return score;
    }

    private PlayerColor Opponent(PlayerColor color)
        => color == PlayerColor.Red
            ? PlayerColor.Green
            : PlayerColor.Red;

    private int EvaluateProgress(Game game, PlayerColor color)
    {
        var bobail = GetBobail(game);

        const int boardSize = 5;
        int targetRow = color == PlayerColor.Red ? 0 : boardSize - 1;

        int distance = Math.Abs(bobail.Position.Row - targetRow);
        int maxDistance = boardSize - 1;
        int progress = maxDistance - distance;

        return progress * progress * _weights.ProgressWeight;
    }

    private int EvaluateEndgamePressure(Game game, PlayerColor color)
    {
        int distance = DistanceToTarget(game, color);

        return distance switch
        {
            0 => 3 * _weights.EndgamePressureWeight,
            1 => 2 * _weights.EndgamePressureWeight,
            2 => _weights.EndgamePressureWeight,
            _ => 0
        };
    }

    private int EvaluateAdjacencyControl(Game game, PlayerColor color)
    {
        var bobail = GetBobail(game);
        int score = 0;

        foreach (var position in Adjacent(bobail.Position))
        {
            var piece = game.Board.GetPieceAt(position);

            if (piece is null)
                continue;

            if (piece.Owner == color)
                score += _weights.FriendlyAdjacencyWeight;
            else
                score -= _weights.OpponentAdjacencyPenaltyWeight;
        }

        return score;
    }

    private int EvaluateCenterControl(Game game, PlayerColor color)
    {
        int score = 0;

        foreach (var piece in game.Board.Pieces)
        {
            if (piece.IsBobail || piece.Owner != color)
                continue;

            if (piece.Position.Row is >= 1 and <= 3 &&
                piece.Position.Column is >= 1 and <= 3)
            {
                score += _weights.CenterControlWeight;
            }
        }

        return score;
    }

    private int EvaluateForwardMobility(Game game, PlayerColor color)
    {
        var moves = game.GetValidBobailMoves();
        int currentDistance = DistanceToTarget(game, color);
        int score = 0;

        foreach (var move in moves)
        {
            int moveDistance = DistanceToTarget(move.Row, color);

            if (moveDistance < currentDistance)
                score += _weights.ForwardMobilityWeight;
        }

        return score;
    }

    private int EvaluateCorridor(Game game, PlayerColor color)
    {
        var bobail = GetBobail(game);
        int score = 0;

        int direction = color == PlayerColor.Red ? -1 : 1;
        int nextRow = bobail.Position.Row + direction;

        if (nextRow >= 0 && nextRow < 5)
        {
            var candidateColumns = new[]
            {
                bobail.Position.Column - 1,
                bobail.Position.Column,
                bobail.Position.Column + 1
            };

            foreach (int column in candidateColumns)
            {
                if (column < 0 || column >= 5)
                    continue;

                var target = new Position(nextRow, column);

                if (game.Board.IsEmpty(target))
                    score += _weights.CorridorWeight;
            }
        }

        return score;
    }

    private int EvaluateImmediateWinThreat(Game game, PlayerColor color)
    {
        if (game.CurrentPhase != TurnPhase.BobailMoveRequired ||
            game.CurrentTurn != color)
        {
            return 0;
        }

        foreach (var move in game.GetValidBobailMoves())
        {
            if (DistanceToTarget(move.Row, color) == 0)
                return _weights.ImmediateWinThreatWeight;
        }

        return 0;
    }

    private static Piece GetBobail(Game game)
    {
        return game.Board.Pieces.First(p => p.IsBobail);
    }

    private static int DistanceToTarget(Game game, PlayerColor color)
    {
        return DistanceToTarget(GetBobail(game).Position.Row, color);
    }

    private static int DistanceToTarget(int row, PlayerColor color)
    {
        int targetRow = color == PlayerColor.Red ? 0 : 4;
        return Math.Abs(row - targetRow);
    }

    private List<Position> Adjacent(Position pos)
    {
        var result = new List<Position>();

        foreach (var direction in AllDirections)
        {
            int row = pos.Row + direction.DeltaRow;
            int column = pos.Column + direction.DeltaColumn;

            if (row < 0 || row >= 5 || column < 0 || column >= 5)
                continue;

            result.Add(new Position(row, column));
        }

        return result;
    }
}
