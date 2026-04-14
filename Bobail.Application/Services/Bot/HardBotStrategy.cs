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

    private const int MaxDepth = 3;
    private const int RootMoveLimit = 5;

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
            takeLimit: RootMoveLimit);

        if (moveData.Count == 0)
            throw new InvalidOperationException("Hard bot has no valid moves.");

        int bestScore = int.MinValue;
        BotMove? bestMove = null;

        foreach (var data in moveData)
        {
            int score = Minimax(
                data.clone,
                MaxDepth - 1,
                maximizingPlayer: false,
                alpha: int.MinValue,
                beta: int.MaxValue,
                botColor: game.CurrentTurn);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = data.move;
            }
        }

        _logger.LogInformation("Hard AI selected move with score {Score}", bestScore);

        return bestMove!;
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
