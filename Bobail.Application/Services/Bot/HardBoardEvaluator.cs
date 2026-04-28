using Bobail.Application.Interfaces.Services;
using Bobail.Application.Services.Bot;
using Bobail.Domain.Board;
using Bobail.Domain.Games;
using Bobail.Domain.Pieces;

namespace Bobail.Infrastructure.Bots;

public class HardBoardEvaluator : IBoardEvaluator
{
    private const int BoardSize = 5;
    private const int MaxPathScore = 10;
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

        score += EvaluatePathToGoal(game, botColor);
        score -= EvaluatePathToGoal(game, opponent);

        score += EvaluateFriendlySupport(game, botColor);
        score -= EvaluateFriendlySupport(game, opponent);

        score += EvaluateOpponentPressure(game, botColor);
        score -= EvaluateOpponentPressure(game, opponent);

        score += EvaluateCenterControl(game, botColor);
        score -= EvaluateCenterControl(game, opponent);

        score += EvaluateBehindBobailFormation(game, botColor);
        score -= EvaluateBehindBobailFormation(game, opponent);

        score += EvaluateTokenDevelopment(game, botColor);
        score -= EvaluateTokenDevelopment(game, opponent);

        score += EvaluateImmediateWinThreat(game, botColor);
        score += EvaluateImmediateLossThreat(game, botColor);
        score += EvaluateBobailMobility(game, botColor);
        score += EvaluateForwardMobility(game, botColor);
        score += EvaluateTrapRisk(game, botColor);
        score += EvaluateDestinationQuality(game, botColor);

        return score;
    }

    private PlayerColor Opponent(PlayerColor color)
        => color == PlayerColor.Red
            ? PlayerColor.Green
            : PlayerColor.Red;

    private int EvaluateCenterControl(Game game, PlayerColor color)
    {
        int control = 0;

        foreach (var piece in GetOwnedPlayerPieces(game, color))
        {
            if (piece.Position.Row == 2 && piece.Position.Column == 2)
            {
                control += 4;
                continue;
            }

            int distanceFromCenter = Math.Abs(piece.Position.Row - 2) + Math.Abs(piece.Position.Column - 2);

            control += distanceFromCenter switch
            {
                1 => 2,
                2 => 1,
                _ => 0
            };
        }

        return control * _weights.CenterControlWeight;
    }

    private int EvaluateBehindBobailFormation(Game game, PlayerColor color)
    {
        var bobail = GetBobail(game);
        int score = 0;

        foreach (var piece in GetOwnedPlayerPieces(game, color))
        {
            int behindDistance = GetBehindBobailDistance(piece.Position.Row, bobail.Position.Row, color);

            if (behindDistance <= 0)
                continue;

            int columnDistance = Math.Abs(piece.Position.Column - bobail.Position.Column);
            int alignmentBonus = columnDistance switch
            {
                0 => 3,
                1 => 2,
                2 => 1,
                _ => 0
            };

            score += alignmentBonus + Math.Max(0, 3 - behindDistance);
        }

        return score * _weights.BehindBobailFormationWeight;
    }

    private int EvaluateTokenDevelopment(Game game, PlayerColor color)
    {
        int score = 0;

        foreach (var piece in GetOwnedPlayerPieces(game, color))
        {
            int distanceFromStartRow = color == PlayerColor.Red
                ? piece.Position.Row
                : BoardSize - 1 - piece.Position.Row;
            int centerColumnBonus = 2 - Math.Abs(piece.Position.Column - 2);
            int mobility = CountAvailablePlayerSlidesFrom(game, piece.Position);

            score += (distanceFromStartRow * 2) + Math.Max(0, centerColumnBonus) + Math.Min(3, mobility);
        }

        return score * _weights.TokenDevelopmentWeight;
    }

    private int EvaluateProgress(Game game, PlayerColor color)
    {
        var bobail = GetBobail(game);

        int targetRow = color == PlayerColor.Red ? 0 : BoardSize - 1;

        int distance = Math.Abs(bobail.Position.Row - targetRow);
        int maxDistance = BoardSize - 1;
        int progress = maxDistance - distance;

        return progress * progress * _weights.ProgressWeight;
    }

    private int EvaluatePathToGoal(Game game, PlayerColor color)
    {
        var bobail = GetBobail(game);
        int? shortestPath = FindShortestPathToTarget(game, bobail.Position, color);

        if (shortestPath is null)
            return -2 * _weights.PathToGoalWeight;

        return Math.Max(0, MaxPathScore - shortestPath.Value) * _weights.PathToGoalWeight;
    }

    private int EvaluateFriendlySupport(Game game, PlayerColor color)
    {
        var bobail = GetBobail(game);
        int currentDistance = DistanceToTarget(game, color);
        var candidateMoves = game.GetValidBobailMoves()
            .Where(move => DistanceToTarget(move.Row, color) <= currentDistance)
            .ToList();

        int score = 0;

        foreach (var position in Adjacent(bobail.Position))
        {
            var piece = game.Board.GetPieceAt(position);

            if (piece is null)
                continue;

            if (!piece.IsBobail && piece.Owner == color)
                score += _weights.FriendlySupportWeight;
        }

        foreach (var move in candidateMoves)
        {
            score += CountAdjacentOwnedPieces(game, move, color) * (_weights.FriendlySupportWeight / 2);
        }

        return score;
    }

    private int EvaluateOpponentPressure(Game game, PlayerColor color)
    {
        var bobail = GetBobail(game);
        int currentDistance = DistanceToTarget(game, color);
        var candidateMoves = game.GetValidBobailMoves()
            .Where(move => DistanceToTarget(move.Row, color) <= currentDistance)
            .ToList();

        int score = 0;

        score -= CountAdjacentOwnedPieces(game, bobail.Position, Opponent(color)) * _weights.OpponentPressureWeight;

        foreach (var move in candidateMoves)
        {
            score -= CountAdjacentOwnedPieces(game, move, Opponent(color)) * (_weights.OpponentPressureWeight / 2);
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

    private int EvaluateImmediateLossThreat(Game game, PlayerColor color)
    {
        if (game.CurrentPhase != TurnPhase.BobailMoveRequired ||
            game.CurrentTurn != Opponent(color))
        {
            return 0;
        }

        foreach (var move in game.GetValidBobailMoves())
        {
            if (DistanceToTarget(move.Row, game.CurrentTurn) == 0)
                return -_weights.ImmediateLossThreatWeight;
        }

        return 0;
    }

    private int EvaluateBobailMobility(Game game, PlayerColor color)
    {
        if (game.CurrentPhase != TurnPhase.BobailMoveRequired)
            return 0;

        int legalMoves = game.GetValidBobailMoves().Count;
        return game.CurrentTurn == color
            ? legalMoves * _weights.BobailMobilityWeight
            : -legalMoves * _weights.BobailMobilityWeight;
    }

    private int EvaluateForwardMobility(Game game, PlayerColor color)
    {
        if (game.CurrentPhase != TurnPhase.BobailMoveRequired)
            return 0;

        PlayerColor activeColor = game.CurrentTurn;
        int currentDistance = DistanceToTarget(game, activeColor);
        int forwardMoves = game.GetValidBobailMoves()
            .Count(move => DistanceToTarget(move.Row, activeColor) < currentDistance);

        return activeColor == color
            ? forwardMoves * _weights.ForwardMobilityWeight
            : -forwardMoves * _weights.ForwardMobilityWeight;
    }

    private int EvaluateTrapRisk(Game game, PlayerColor color)
    {
        if (game.CurrentPhase != TurnPhase.BobailMoveRequired)
            return 0;

        PlayerColor activeColor = game.CurrentTurn;
        var bobail = GetBobail(game);
        var legalMoves = game.GetValidBobailMoves();

        int lowMobilityRisk = legalMoves.Count switch
        {
            0 => 5,
            1 => 4,
            2 => 2,
            3 => 1,
            _ => 0
        };

        int deadEndRisk = legalMoves.Count(move =>
            CountAvailableBobailMovesFrom(game, bobail.Position, move) <= 1);

        int edgeRisk = IsCorner(bobail.Position) ? 2 : IsEdge(bobail.Position) ? 1 : 0;
        int totalRisk = lowMobilityRisk + deadEndRisk + edgeRisk;

        return activeColor == color
            ? -totalRisk * _weights.TrapRiskWeight
            : totalRisk * _weights.TrapRiskWeight;
    }

    private int EvaluateDestinationQuality(Game game, PlayerColor color)
    {
        if (game.CurrentPhase != TurnPhase.BobailMoveRequired)
            return 0;

        PlayerColor activeColor = game.CurrentTurn;
        var bobail = GetBobail(game);
        var legalMoves = game.GetValidBobailMoves();

        if (legalMoves.Count == 0)
            return 0;

        int currentDistance = DistanceToTarget(game, activeColor);
        int totalQuality = 0;
        int bestQuality = int.MinValue;

        foreach (var move in legalMoves)
        {
            int moveDistance = DistanceToTarget(move.Row, activeColor);
            int progressDelta = currentDistance - moveDistance;
            int futureMobility = CountAvailableBobailMovesFrom(game, bobail.Position, move);
            int support = CountAdjacentOwnedPieces(game, move, activeColor);
            int pressure = CountAdjacentOwnedPieces(game, move, Opponent(activeColor));
            int centerBonus = move.Row is >= 1 and <= 3 && move.Column is >= 1 and <= 3 ? 1 : 0;

            int quality = (progressDelta * 2) + futureMobility + support - pressure + centerBonus;
            totalQuality += quality;
            bestQuality = Math.Max(bestQuality, quality);
        }

        int blendedQuality = bestQuality + (totalQuality / legalMoves.Count);

        return activeColor == color
            ? blendedQuality * _weights.DestinationQualityWeight
            : -blendedQuality * _weights.DestinationQualityWeight;
    }

    private static Piece GetBobail(Game game)
    {
        return game.Board.Pieces.First(p => p.IsBobail);
    }

    private static IEnumerable<Piece> GetOwnedPlayerPieces(Game game, PlayerColor color)
    {
        return game.Board.Pieces
            .Where(piece => !piece.IsBobail && piece.Owner == color);
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

    private static int GetBehindBobailDistance(int pieceRow, int bobailRow, PlayerColor color)
    {
        return color == PlayerColor.Red
            ? pieceRow - bobailRow
            : bobailRow - pieceRow;
    }

    private int? FindShortestPathToTarget(Game game, Position start, PlayerColor color)
    {
        int targetRow = color == PlayerColor.Red ? 0 : BoardSize - 1;
        var visited = new HashSet<Position> { start };
        var queue = new Queue<(Position position, int distance)>();
        queue.Enqueue((start, 0));

        while (queue.Count > 0)
        {
            var (position, distance) = queue.Dequeue();

            if (position.Row == targetRow)
                return distance;

            foreach (var next in Adjacent(position))
            {
                if (!visited.Add(next))
                    continue;

                if (!game.Board.IsEmpty(next) && !next.Equals(start))
                    continue;

                queue.Enqueue((next, distance + 1));
            }
        }

        return null;
    }

    private int CountAdjacentOwnedPieces(Game game, Position position, PlayerColor color)
    {
        int count = 0;

        foreach (var adjacent in Adjacent(position))
        {
            var piece = game.Board.GetPieceAt(adjacent);

            if (piece is not null && !piece.IsBobail && piece.Owner == color)
                count++;
        }

        return count;
    }

    private int CountAvailableBobailMovesFrom(Game game, Position origin, Position bobailPosition)
    {
        int count = 0;

        foreach (var adjacent in Adjacent(bobailPosition))
        {
            if (adjacent.Equals(origin))
            {
                count++;
                continue;
            }

            if (game.Board.IsEmpty(adjacent))
                count++;
        }

        return count;
    }

    private int CountAvailablePlayerSlidesFrom(Game game, Position from)
    {
        int count = 0;

        foreach (var direction in AllDirections)
        {
            var destination = GetFarthestEmptyPosition(game, from, direction);

            if (!destination.Equals(from))
                count++;
        }

        return count;
    }

    private static Position GetFarthestEmptyPosition(Game game, Position from, Direction direction)
    {
        int row = from.Row;
        int column = from.Column;

        while (true)
        {
            int nextRow = row + direction.DeltaRow;
            int nextColumn = column + direction.DeltaColumn;

            if (nextRow < 0 || nextRow >= BoardSize || nextColumn < 0 || nextColumn >= BoardSize)
                break;

            var next = new Position(nextRow, nextColumn);

            if (!game.Board.IsEmpty(next))
                break;

            row = nextRow;
            column = nextColumn;
        }

        return new Position(row, column);
    }

    private static bool IsEdge(Position position)
    {
        return position.Row == 0 ||
               position.Row == BoardSize - 1 ||
               position.Column == 0 ||
               position.Column == BoardSize - 1;
    }

    private static bool IsCorner(Position position)
    {
        return (position.Row == 0 || position.Row == BoardSize - 1) &&
               (position.Column == 0 || position.Column == BoardSize - 1);
    }

    private List<Position> Adjacent(Position pos)
    {
        var result = new List<Position>();

        foreach (var direction in AllDirections)
        {
            int row = pos.Row + direction.DeltaRow;
            int column = pos.Column + direction.DeltaColumn;

            if (row < 0 || row >= BoardSize || column < 0 || column >= BoardSize)
                continue;

            result.Add(new Position(row, column));
        }

        return result;
    }
}
