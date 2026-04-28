using Bobail.Application.Interfaces.Services;
using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;
using Microsoft.Extensions.Logging;

namespace Bobail.Infrastructure.Bots;

public class MediumBotStrategy : IBotStrategy
{
    private readonly record struct SearchCacheKey(
        string State,
        int Depth,
        bool MaximizingPlayer,
        PlayerColor BotColor);

    private readonly MediumBoardEvaluator _evaluator;
    private readonly ILogger<MediumBotStrategy> _logger;
    private readonly Dictionary<SearchCacheKey, int> _transpositionTable = new();

    private const int MaxDepth = 2;
    private const int RootMoveLimit = 5;
    private const int CandidatePoolSize = 4;
    private const int ScoreWindow = 1_200;
    private const double ImperfectChoiceChance = 0.4;

    public BotDifficulty Difficulty => BotDifficulty.Medium;

    public MediumBotStrategy(
        MediumBoardEvaluator evaluator,
        ILogger<MediumBotStrategy> logger)
    {
        _evaluator = evaluator;
        _logger = logger;
    }

    public BotMove DecideMove(Game game)
    {
        _logger.LogInformation("Medium AI started calculation. Depth: {Depth}", MaxDepth);

        _transpositionTable.Clear();

        var moveData = PrepareOrderedMoves(
            game,
            botColor: game.CurrentTurn,
            maximizingPlayer: true,
            takeLimit: RootMoveLimit);

        if (moveData.Count == 0)
            throw new InvalidOperationException("Medium bot has no valid moves.");

        var scoredMoves = new List<(BotMove move, int score)>(moveData.Count);

        foreach (var data in moveData)
        {
            int score = Minimax(
                data.clone,
                MaxDepth - 1,
                alpha: int.MinValue,
                beta: int.MaxValue,
                botColor: game.CurrentTurn);

            scoredMoves.Add((data.move, score));
        }

        int bestScore = scoredMoves.Max(x => x.score);

        var candidateMoves = scoredMoves
            .Where(x => bestScore - x.score <= ScoreWindow)
            .OrderByDescending(x => x.score)
            .Take(CandidatePoolSize)
            .ToList();

        var selectedMove = ChooseMediumMove(candidateMoves);

        _logger.LogInformation(
            "Medium AI selected move with score {SelectedScore} from {CandidateCount} near-best candidates. Best score: {BestScore}",
            selectedMove.score,
            candidateMoves.Count,
            bestScore);

        return selectedMove.move;
    }

    private int Minimax(
        Game game,
        int depth,
        int alpha,
        int beta,
        PlayerColor botColor)
    {
        if (depth == 0 || game.Status == GameStatus.Finished)
            return _evaluator.Evaluate(game, botColor);

        bool maximizingPlayer = game.CurrentTurn == botColor;

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

    private static List<BotMove> GenerateAllMoves(Game game)
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

    private static void ApplyMove(Game clone, BotMove move)
    {
        if (move.IsBobailMove)
            clone.ExecuteBobailMove(move.To);
        else
            clone.ExecutePlayerMove(move.From, move.To);
    }

    private static (BotMove move, int score) ChooseMediumMove(IReadOnlyList<(BotMove move, int score)> candidateMoves)
    {
        if (candidateMoves.Count == 1)
            return candidateMoves[0];

        if (Random.Shared.NextDouble() >= ImperfectChoiceChance)
            return candidateMoves[0];

        var alternativeMoves = candidateMoves.Skip(1).ToList();

        if (alternativeMoves.Count == 0)
            return candidateMoves[0];

        int bestScore = candidateMoves[0].score;
        var weightedAlternatives = alternativeMoves
            .Select(candidate =>
            {
                int distanceFromBest = Math.Max(1, bestScore - candidate.score);
                double weight = 1.0 / distanceFromBest;
                return (candidate.move, candidate.score, weight);
            })
            .ToList();

        double totalWeight = weightedAlternatives.Sum(x => x.weight);
        double roll = Random.Shared.NextDouble() * totalWeight;

        foreach (var candidate in weightedAlternatives)
        {
            roll -= candidate.weight;

            if (roll <= 0)
                return (candidate.move, candidate.score);
        }

        var fallback = weightedAlternatives[^1];
        return (fallback.move, fallback.score);
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
