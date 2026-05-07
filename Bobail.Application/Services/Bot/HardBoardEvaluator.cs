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
    private static readonly Position[][] AdjacentPositions = CreateAdjacentPositions();

    private const int WinScore = 1_000_000;
    private const int LossScore = -1_000_000;
    private readonly EvaluationWeights _weights;

    private sealed class EvaluationContext
    {
        private readonly Piece?[] _piecesBySquare = new Piece?[BoardSize * BoardSize];

        public EvaluationContext(Game game)
        {
            Game = game;
            Piece? bobail = null;

            foreach (var piece in game.Board.Pieces)
            {
                _piecesBySquare[ToIndex(piece.Position)] = piece;

                if (piece.IsBobail)
                {
                    bobail = piece;
                }
                else if (piece.Owner == PlayerColor.Red)
                {
                    RedPieces.Add(piece);
                }
                else
                {
                    GreenPieces.Add(piece);
                }
            }

            if (bobail is null)
                throw new InvalidOperationException("Bobail not found on board.");

            Bobail = bobail;
            ValidBobailMoves = BuildValidBobailMoves();
        }

        public Game Game { get; }

        public Piece Bobail { get; }

        public List<Position> ValidBobailMoves { get; }

        private List<Piece> RedPieces { get; } = new(BoardSize);

        private List<Piece> GreenPieces { get; } = new(BoardSize);

        public IEnumerable<Piece> GetOwnedPieces(PlayerColor color)
        {
            return color == PlayerColor.Red ? RedPieces : GreenPieces;
        }

        public Piece? GetPieceAt(Position position)
        {
            return _piecesBySquare[ToIndex(position)];
        }

        public bool IsEmpty(Position position)
        {
            return GetPieceAt(position) is null;
        }

        private List<Position> BuildValidBobailMoves()
        {
            var validMoves = new List<Position>();

            foreach (var target in Adjacent(Bobail.Position))
            {
                if (IsEmpty(target))
                    validMoves.Add(target);
            }

            return validMoves;
        }
    }

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

        var context = new EvaluationContext(game);
        var opponent = Opponent(botColor);

        int score = 0;

        score += EvaluateProgress(context, botColor);
        score -= EvaluateProgress(context, opponent);

        score += EvaluatePathToGoal(context, botColor);
        score -= EvaluatePathToGoal(context, opponent);

        score += EvaluateFriendlySupport(context, botColor);
        score -= EvaluateFriendlySupport(context, opponent);

        score += EvaluateOpponentPressure(context, botColor);
        score -= EvaluateOpponentPressure(context, opponent);

        score += EvaluateCenterControl(context, botColor);
        score -= EvaluateCenterControl(context, opponent);

        score += EvaluateBehindBobailFormation(context, botColor);
        score -= EvaluateBehindBobailFormation(context, opponent);

        score += EvaluateTokenDevelopment(context, botColor);
        score -= EvaluateTokenDevelopment(context, opponent);

        score += EvaluateImmediateWinThreat(context, botColor);
        score += EvaluateImmediateLossThreat(context, botColor);
        score += EvaluateBobailMobility(context, botColor);
        score += EvaluateForwardMobility(context, botColor);
        score += EvaluateTrapRisk(context, botColor);
        score += EvaluateDestinationQuality(context, botColor);

        return score;
    }

    private PlayerColor Opponent(PlayerColor color)
        => color == PlayerColor.Red
            ? PlayerColor.Green
            : PlayerColor.Red;

    private int EvaluateCenterControl(EvaluationContext context, PlayerColor color)
    {
        int control = 0;

        foreach (var piece in context.GetOwnedPieces(color))
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

    private int EvaluateBehindBobailFormation(EvaluationContext context, PlayerColor color)
    {
        var bobail = context.Bobail;
        int score = 0;

        foreach (var piece in context.GetOwnedPieces(color))
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

    private int EvaluateTokenDevelopment(EvaluationContext context, PlayerColor color)
    {
        int score = 0;

        foreach (var piece in context.GetOwnedPieces(color))
        {
            int distanceFromStartRow = color == PlayerColor.Red
                ? piece.Position.Row
                : BoardSize - 1 - piece.Position.Row;
            int centerColumnBonus = 2 - Math.Abs(piece.Position.Column - 2);
            int mobility = CountAvailablePlayerSlidesFrom(context, piece.Position);

            score += (distanceFromStartRow * 2) + Math.Max(0, centerColumnBonus) + Math.Min(3, mobility);
        }

        return score * _weights.TokenDevelopmentWeight;
    }

    private int EvaluateProgress(EvaluationContext context, PlayerColor color)
    {
        int targetRow = color == PlayerColor.Red ? 0 : BoardSize - 1;

        int distance = Math.Abs(context.Bobail.Position.Row - targetRow);
        int maxDistance = BoardSize - 1;
        int progress = maxDistance - distance;

        return progress * progress * _weights.ProgressWeight;
    }

    private int EvaluatePathToGoal(EvaluationContext context, PlayerColor color)
    {
        int? shortestPath = FindShortestPathToTarget(context, context.Bobail.Position, color);

        if (shortestPath is null)
            return -2 * _weights.PathToGoalWeight;

        return Math.Max(0, MaxPathScore - shortestPath.Value) * _weights.PathToGoalWeight;
    }

    private int EvaluateFriendlySupport(EvaluationContext context, PlayerColor color)
    {
        var bobail = context.Bobail;
        int currentDistance = DistanceToTarget(context, color);

        int score = 0;

        foreach (var position in Adjacent(bobail.Position))
        {
            var piece = context.GetPieceAt(position);

            if (piece is null)
                continue;

            if (!piece.IsBobail && piece.Owner == color)
                score += _weights.FriendlySupportWeight;
        }

        foreach (var move in context.ValidBobailMoves)
        {
            if (DistanceToTarget(move.Row, color) <= currentDistance)
                score += CountAdjacentOwnedPieces(context, move, color) * (_weights.FriendlySupportWeight / 2);
        }

        return score;
    }

    private int EvaluateOpponentPressure(EvaluationContext context, PlayerColor color)
    {
        var bobail = context.Bobail;
        int currentDistance = DistanceToTarget(context, color);
        var opponent = Opponent(color);

        int score = 0;

        score -= CountAdjacentOwnedPieces(context, bobail.Position, opponent) * _weights.OpponentPressureWeight;

        foreach (var move in context.ValidBobailMoves)
        {
            if (DistanceToTarget(move.Row, color) <= currentDistance)
                score -= CountAdjacentOwnedPieces(context, move, opponent) * (_weights.OpponentPressureWeight / 2);
        }

        return score;
    }

    private int EvaluateImmediateWinThreat(EvaluationContext context, PlayerColor color)
    {
        if (context.Game.CurrentPhase != TurnPhase.BobailMoveRequired ||
            context.Game.CurrentTurn != color)
        {
            return 0;
        }

        foreach (var move in context.ValidBobailMoves)
        {
            if (DistanceToTarget(move.Row, color) == 0)
                return _weights.ImmediateWinThreatWeight;
        }

        return 0;
    }

    private int EvaluateImmediateLossThreat(EvaluationContext context, PlayerColor color)
    {
        if (context.Game.CurrentPhase != TurnPhase.BobailMoveRequired ||
            context.Game.CurrentTurn != Opponent(color))
        {
            return 0;
        }

        foreach (var move in context.ValidBobailMoves)
        {
            if (DistanceToTarget(move.Row, context.Game.CurrentTurn) == 0)
                return -_weights.ImmediateLossThreatWeight;
        }

        return 0;
    }

    private int EvaluateBobailMobility(EvaluationContext context, PlayerColor color)
    {
        if (context.Game.CurrentPhase != TurnPhase.BobailMoveRequired)
            return 0;

        int legalMoves = context.ValidBobailMoves.Count;
        return context.Game.CurrentTurn == color
            ? legalMoves * _weights.BobailMobilityWeight
            : -legalMoves * _weights.BobailMobilityWeight;
    }

    private int EvaluateForwardMobility(EvaluationContext context, PlayerColor color)
    {
        if (context.Game.CurrentPhase != TurnPhase.BobailMoveRequired)
            return 0;

        PlayerColor activeColor = context.Game.CurrentTurn;
        int currentDistance = DistanceToTarget(context, activeColor);
        int forwardMoves = 0;

        foreach (var move in context.ValidBobailMoves)
        {
            if (DistanceToTarget(move.Row, activeColor) < currentDistance)
                forwardMoves++;
        }

        return activeColor == color
            ? forwardMoves * _weights.ForwardMobilityWeight
            : -forwardMoves * _weights.ForwardMobilityWeight;
    }

    private int EvaluateTrapRisk(EvaluationContext context, PlayerColor color)
    {
        if (context.Game.CurrentPhase != TurnPhase.BobailMoveRequired)
            return 0;

        PlayerColor activeColor = context.Game.CurrentTurn;
        var bobail = context.Bobail;
        var legalMoves = context.ValidBobailMoves;

        int lowMobilityRisk = legalMoves.Count switch
        {
            0 => 5,
            1 => 4,
            2 => 2,
            3 => 1,
            _ => 0
        };

        int deadEndRisk = 0;

        foreach (var move in legalMoves)
        {
            if (CountAvailableBobailMovesFrom(context, bobail.Position, move) <= 1)
                deadEndRisk++;
        }

        int edgeRisk = IsCorner(bobail.Position) ? 2 : IsEdge(bobail.Position) ? 1 : 0;
        int totalRisk = lowMobilityRisk + deadEndRisk + edgeRisk;

        return activeColor == color
            ? -totalRisk * _weights.TrapRiskWeight
            : totalRisk * _weights.TrapRiskWeight;
    }

    private int EvaluateDestinationQuality(EvaluationContext context, PlayerColor color)
    {
        if (context.Game.CurrentPhase != TurnPhase.BobailMoveRequired)
            return 0;

        PlayerColor activeColor = context.Game.CurrentTurn;
        var bobail = context.Bobail;
        var legalMoves = context.ValidBobailMoves;

        if (legalMoves.Count == 0)
            return 0;

        int currentDistance = DistanceToTarget(context, activeColor);
        int totalQuality = 0;
        int bestQuality = int.MinValue;

        foreach (var move in legalMoves)
        {
            int moveDistance = DistanceToTarget(move.Row, activeColor);
            int progressDelta = currentDistance - moveDistance;
            int futureMobility = CountAvailableBobailMovesFrom(context, bobail.Position, move);
            int support = CountAdjacentOwnedPieces(context, move, activeColor);
            int pressure = CountAdjacentOwnedPieces(context, move, Opponent(activeColor));
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

    private static int DistanceToTarget(EvaluationContext context, PlayerColor color)
    {
        return DistanceToTarget(context.Bobail.Position.Row, color);
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

    private int? FindShortestPathToTarget(EvaluationContext context, Position start, PlayerColor color)
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

                if (!context.IsEmpty(next) && !next.Equals(start))
                    continue;

                queue.Enqueue((next, distance + 1));
            }
        }

        return null;
    }

    private int CountAdjacentOwnedPieces(EvaluationContext context, Position position, PlayerColor color)
    {
        int count = 0;

        foreach (var adjacent in Adjacent(position))
        {
            var piece = context.GetPieceAt(adjacent);

            if (piece is not null && !piece.IsBobail && piece.Owner == color)
                count++;
        }

        return count;
    }

    private int CountAvailableBobailMovesFrom(EvaluationContext context, Position origin, Position bobailPosition)
    {
        int count = 0;

        foreach (var adjacent in Adjacent(bobailPosition))
        {
            if (adjacent.Equals(origin))
            {
                count++;
                continue;
            }

            if (context.IsEmpty(adjacent))
                count++;
        }

        return count;
    }

    private int CountAvailablePlayerSlidesFrom(EvaluationContext context, Position from)
    {
        int count = 0;

        foreach (var direction in AllDirections)
        {
            var destination = GetFarthestEmptyPosition(context, from, direction);

            if (!destination.Equals(from))
                count++;
        }

        return count;
    }

    private static Position GetFarthestEmptyPosition(EvaluationContext context, Position from, Direction direction)
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

            if (!context.IsEmpty(next))
                break;

            row = nextRow;
            column = nextColumn;
        }

        return new Position(row, column);
    }

    private static int ToIndex(Position position)
    {
        return (position.Row * BoardSize) + position.Column;
    }

    private static int ToIndex(int row, int column)
    {
        return (row * BoardSize) + column;
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

    private static Position[] Adjacent(Position pos)
    {
        return AdjacentPositions[ToIndex(pos)];
    }

    private static Position[][] CreateAdjacentPositions()
    {
        var adjacentPositions = new Position[BoardSize * BoardSize][];

        for (int sourceRow = 0; sourceRow < BoardSize; sourceRow++)
        {
            for (int sourceColumn = 0; sourceColumn < BoardSize; sourceColumn++)
            {
                var positions = new List<Position>(AllDirections.Length);

                foreach (var direction in AllDirections)
                {
                    int row = sourceRow + direction.DeltaRow;
                    int column = sourceColumn + direction.DeltaColumn;

                    if (row < 0 || row >= BoardSize || column < 0 || column >= BoardSize)
                        continue;

                    positions.Add(new Position(row, column));
                }

                adjacentPositions[ToIndex(sourceRow, sourceColumn)] = positions.ToArray();
            }
        }

        return adjacentPositions;
    }
}
