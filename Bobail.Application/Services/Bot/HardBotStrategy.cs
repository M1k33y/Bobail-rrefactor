using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;
using Microsoft.Extensions.Logging;

namespace Bobail.Infrastructure.Bots;

public class HardBotStrategy : IBotStrategy
{
    private readonly record struct SearchCacheKey(
        string State,
        int Depth,
        bool MaximizingPlayer,
        PlayerColor BotColor);

    private readonly HardBoardEvaluator _evaluator;
    private readonly ILogger<HardBotStrategy> _logger;
    private readonly Dictionary<SearchCacheKey, int> _transpositionTable = new();

    public BotDifficulty Difficulty => BotDifficulty.Hard;

    private const int RootMoveLimit = 6;
    private const int MaxDepth = 3;
    private const int CandidatePoolSize = 3;
    private const int ScoreWindow = 250;
    private const double NearBestRandomizerChance = 0.05;
    private const int ImmediateThreatPenalty = 250_000;
    private const int ForcedWinBonus = 180_000;
    private const int BackwardBobailPenalty = 1_400;

    public HardBotStrategy(
        HardBoardEvaluator evaluator,
        ILogger<HardBotStrategy> logger)
    {
        _evaluator = evaluator;
        _logger = logger;
    }

    public BotMove DecideMove(Game game)
    {
        _logger.LogInformation("Hard AI started calculation. Depth: {Depth}", MaxDepth);

        _transpositionTable.Clear();

        var moveData = PrepareOrderedMoves(
            game,
            botColor: game.CurrentTurn,
            maximizingPlayer: true,
            takeLimit: RootMoveLimit); //de pus RootMoveLimit daca e cazul

        if (moveData.Count == 0)
            throw new InvalidOperationException("Hard bot has no valid moves.");

        var scoredMoves = new List<(BotMove move, int score)>(moveData.Count);

        foreach (var data in moveData)
        {
            int score = Minimax(
                data.clone,
                MaxDepth - 1,
                maximizingPlayer: false,
                alpha: int.MinValue,
                beta: int.MaxValue,
                botColor: game.CurrentTurn);

            score += EvaluateTacticalAdjustments(game, data.move, data.clone, game.CurrentTurn);
            scoredMoves.Add((data.move, score));
        }

        int bestScore = scoredMoves.Max(x => x.score);
        var candidateMoves = scoredMoves
            .Where(x => bestScore - x.score <= ScoreWindow)
            .OrderByDescending(x => x.score)
            .Take(CandidatePoolSize)
            .ToList();

        var selectedMove = ChooseNearBestMove(candidateMoves);

        _logger.LogInformation(
            "Hard AI selected move with score {Score} from {CandidateCount} candidates. Best score: {BestScore}",
            selectedMove.score,
            candidateMoves.Count,
            bestScore);

        return selectedMove.move;
    }

    private int Minimax(
        Game game,
        int depth,
        bool maximizingPlayer,
        int alpha,
        int beta,
        PlayerColor botColor)
    {
        if (depth == 0 || game.Status == GameStatus.Finished)
            return _evaluator.Evaluate(game, botColor);

        var cacheKey = new SearchCacheKey(
            BuildStateKey(game),
            depth,
            maximizingPlayer,
            botColor);

        if (_transpositionTable.TryGetValue(cacheKey, out int cachedScore))
            return cachedScore;

        var moveData = PrepareOrderedMoves(
            game,
            botColor,
            maximizingPlayer,
            takeLimit: null);

        if (moveData.Count == 0)
        {
            int noMoveScore = game.CurrentTurn == botColor ? -1_000_000 : 1_000_000;
            _transpositionTable[cacheKey] = noMoveScore;
            return noMoveScore;
        }

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;

            foreach (var data in moveData)
            {
                int eval = Minimax(
                    data.clone,
                    depth - 1,
                    maximizingPlayer: false,
                    alpha,
                    beta,
                    botColor);

                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);

                if (beta <= alpha)
                    break;
            }

            _transpositionTable[cacheKey] = maxEval;
            return maxEval;
        }

        int minEval = int.MaxValue;

        foreach (var data in moveData)
        {
            int eval = Minimax(
                data.clone,
                depth - 1,
                maximizingPlayer: true,
                alpha,
                beta,
                botColor);

            minEval = Math.Min(minEval, eval);
            beta = Math.Min(beta, eval);

            if (beta <= alpha)
                break;
        }

        _transpositionTable[cacheKey] = minEval;
        return minEval;
    }

    private List<(BotMove move, Game clone, int score)> PrepareOrderedMoves(
        Game game,
        PlayerColor botColor,
        bool maximizingPlayer,
        int? takeLimit)
    {
        var moveData = new List<(BotMove move, Game clone, int score)>();

        foreach (var move in GenerateAllMoves(game))
        {
            var clone = game.Clone();
            ApplyMove(clone, move);

            int score = _evaluator.Evaluate(clone, botColor);
            moveData.Add((move, clone, score));
        }

        IEnumerable<(BotMove move, Game clone, int score)> orderedMoves = maximizingPlayer
            ? moveData.OrderByDescending(x => x.score)
            : moveData.OrderBy(x => x.score);

        if (takeLimit.HasValue)
            orderedMoves = orderedMoves.Take(takeLimit.Value);

        return orderedMoves.ToList();
    }

    private int EvaluateTacticalAdjustments(
        Game originalGame,
        BotMove move,
        Game gameAfterMove,
        PlayerColor botColor)
    {
        int score = 0;

        if (AllowsImmediateOpponentWin(gameAfterMove, botColor))
            score -= ImmediateThreatPenalty;

        if (ShouldEvaluateForcedWin(gameAfterMove, botColor) &&
            CreatesForcedWinNextTurn(gameAfterMove, botColor))
            score += ForcedWinBonus;

        if (move.IsBobailMove)
            score -= EvaluateBackwardBobailPenalty(originalGame, move, botColor);

        return score;
    }

    private List<BotMove> GenerateAllMoves(Game game)
    {
        var moves = new List<BotMove>();

        if (game.CurrentPhase == TurnPhase.BobailMoveRequired)
        {
            foreach (var move in game.GetValidBobailMoves())
                moves.Add(BotMove.Bobail(move));
        }
        else
        {
            var pieces = game.Board.Pieces
                .Where(p => !p.IsBobail && p.Owner == game.CurrentTurn);

            foreach (var piece in pieces)
            {
                var validMoves = game.GetValidPlayerMoves(piece.Position);

                foreach (var move in validMoves)
                    moves.Add(BotMove.Piece(piece.Position, move));
            }
        }

        return moves;
    }

    private void ApplyMove(Game clone, BotMove move)
    {
        if (move.IsBobailMove)
            clone.ExecuteBobailMove(move.To);
        else
            clone.ExecutePlayerMove(move.From, move.To);
    }

    private static bool AllowsImmediateOpponentWin(Game gameAfterMove, PlayerColor botColor)
    {
        if (gameAfterMove.Status == GameStatus.Finished)
            return false;

        var opponent = Opponent(botColor);

        if (gameAfterMove.CurrentTurn != opponent)
            return false;

        foreach (var move in GenerateAllMovesForColor(gameAfterMove, opponent))
        {
            var clone = gameAfterMove.Clone();
            ApplyMoveStatic(clone, move);

            if (clone.Status == GameStatus.Finished && clone.Winner == opponent)
                return true;
        }

        return false;
    }

    private static bool CreatesForcedWinNextTurn(Game gameAfterMove, PlayerColor botColor)
    {
        if (gameAfterMove.Status == GameStatus.Finished)
            return false;

        var opponent = Opponent(botColor);
        var opponentMoves = GenerateAllMovesForColor(gameAfterMove, opponent);

        if (opponentMoves.Count == 0)
            return false;

        foreach (var opponentMove in opponentMoves)
        {
            var replyState = gameAfterMove.Clone();
            ApplyMoveStatic(replyState, opponentMove);

            if (replyState.Status == GameStatus.Finished)
            {
                if (replyState.Winner != botColor)
                    return false;

                continue;
            }

            if (replyState.CurrentTurn != botColor)
                return false;

            bool hasWinningReply = GenerateAllMovesForColor(replyState, botColor)
                .Any(replyMove =>
                {
                    var winCheck = replyState.Clone();
                    ApplyMoveStatic(winCheck, replyMove);
                    return winCheck.Status == GameStatus.Finished && winCheck.Winner == botColor;
                });

            if (!hasWinningReply)
                return false;
        }

        return true;
    }

    private static bool ShouldEvaluateForcedWin(Game gameAfterMove, PlayerColor botColor)
    {
        var bobail = gameAfterMove.Board.Pieces.First(p => p.IsBobail);
        return DistanceToTarget(bobail.Position.Row, botColor) <= 1;
    }

    private static int EvaluateBackwardBobailPenalty(Game originalGame, BotMove move, PlayerColor botColor)
    {
        var bobail = originalGame.Board.Pieces.First(p => p.IsBobail);
        int currentDistance = DistanceToTarget(bobail.Position.Row, botColor);
        int nextDistance = DistanceToTarget(move.To.Row, botColor);

        return nextDistance > currentDistance
            ? (nextDistance - currentDistance) * BackwardBobailPenalty
            : 0;
    }

    private static (BotMove move, int score) ChooseNearBestMove(IReadOnlyList<(BotMove move, int score)> candidateMoves)
    {
        if (candidateMoves.Count == 1 || Random.Shared.NextDouble() >= NearBestRandomizerChance)
            return candidateMoves[0];

        var alternatives = candidateMoves.Skip(1).ToList();

        if (alternatives.Count == 0)
            return candidateMoves[0];

        double totalWeight = alternatives
            .Sum(candidate => 1.0 / Math.Max(1, candidateMoves[0].score - candidate.score));

        double roll = Random.Shared.NextDouble() * totalWeight;

        foreach (var candidate in alternatives)
        {
            roll -= 1.0 / Math.Max(1, candidateMoves[0].score - candidate.score);

            if (roll <= 0)
                return candidate;
        }

        return alternatives[^1];
    }

    private static List<BotMove> GenerateAllMovesForColor(Game game, PlayerColor color)
    {
        if (game.CurrentTurn != color)
            return new List<BotMove>();

        var moves = new List<BotMove>();

        if (game.CurrentPhase == TurnPhase.BobailMoveRequired)
        {
            foreach (var move in game.GetValidBobailMoves())
                moves.Add(BotMove.Bobail(move));

            return moves;
        }

        var pieces = game.Board.Pieces
            .Where(p => !p.IsBobail && p.Owner == color);

        foreach (var piece in pieces)
        {
            foreach (var move in game.GetValidPlayerMoves(piece.Position))
                moves.Add(BotMove.Piece(piece.Position, move));
        }

        return moves;
    }

    private static void ApplyMoveStatic(Game clone, BotMove move)
    {
        if (move.IsBobailMove)
            clone.ExecuteBobailMove(move.To);
        else
            clone.ExecutePlayerMove(move.From, move.To);
    }

    private static PlayerColor Opponent(PlayerColor color)
    {
        return color == PlayerColor.Red ? PlayerColor.Green : PlayerColor.Red;
    }

    private static int DistanceToTarget(int row, PlayerColor color)
    {
        int targetRow = color == PlayerColor.Red ? 0 : 4;
        return Math.Abs(row - targetRow);
    }

    private static string BuildStateKey(Game game)
    {
        var pieces = game.Board.Pieces
            .OrderBy(p => p.IsBobail ? 0 : 1)
            .ThenBy(p => p.IsBobail ? -1 : (int)(p.Owner ?? throw new InvalidOperationException("Non-Bobail piece must have an owner.")))
            .ThenBy(p => p.Position.Row)
            .ThenBy(p => p.Position.Column)
            .Select(p =>
            {
                string owner = p.IsBobail
                    ? "B"
                    : ((int)(p.Owner ?? throw new InvalidOperationException("Non-Bobail piece must have an owner."))).ToString();
                return $"{owner}:{p.Position.Row},{p.Position.Column}";
            });

        return string.Join("|", pieces) +
               $"|T:{(int)game.CurrentTurn}|P:{(int)game.CurrentPhase}|S:{(int)game.Status}";
    }
}
