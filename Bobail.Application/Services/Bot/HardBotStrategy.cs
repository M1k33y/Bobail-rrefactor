using Bobail.Application.Interfaces.Services;
using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;
using Microsoft.Extensions.Logging;

namespace Bobail.Infrastructure.Bots;

public class HardBotStrategy : IBotStrategy
{
    private readonly HardBoardEvaluator _evaluator;
    private readonly ILogger<HardBotStrategy> _logger;

    public BotDifficulty Difficulty => BotDifficulty.Hard;

    private const int MaxDepth = 3;

  
    private const int RootMoveLimit = 8;
    private const int InnerMoveLimit = 6;

    private static readonly Random _random = new();

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

        var moveData = PrepareOrderedMoves(game, game.CurrentTurn, RootMoveLimit);

        int bestScore = int.MinValue;
        var bestMoves = new List<BotMove>();

        foreach (var data in moveData)
        {
            int score = Minimax(
                data.clone,
                MaxDepth - 1,
                false,
                int.MinValue,
                int.MaxValue,
                game.CurrentTurn);

            if (score > bestScore)
            {
                bestScore = score;
                bestMoves.Clear();
                bestMoves.Add(data.move);
            }
            else if (Math.Abs(score - bestScore) < 200)
            {
                bestMoves.Add(data.move);
            }
        }

        _logger.LogInformation("Hard AI selected move with score {Score}", bestScore);

        return bestMoves[_random.Next(bestMoves.Count)];
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

        var moveData = PrepareOrderedMoves(game, botColor, InnerMoveLimit);

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;

            foreach (var data in moveData)
            {
                int eval = Minimax(
                    data.clone,
                    depth - 1,
                    false,
                    alpha,
                    beta,
                    botColor);

                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);

                if (beta <= alpha)
                    break;
            }

            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;

            foreach (var data in moveData)
            {
                int eval = Minimax(
                    data.clone,
                    depth - 1,
                    true,
                    alpha,
                    beta,
                    botColor);

                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);

                if (beta <= alpha)
                    break;
            }

            return minEval;
        }
    }


    private List<(BotMove move, Game clone, int score)> PrepareOrderedMoves(
        Game game,
        PlayerColor botColor,
        int takeLimit)
    {
        var moveData = new List<(BotMove move, Game clone, int score)>();

        foreach (var move in GenerateAllMoves(game))
        {
            var clone = game.Clone();
            ApplyMove(clone, move);

            int score = _evaluator.Evaluate(clone, botColor);

            moveData.Add((move, clone, score));
        }

        return moveData
            .OrderByDescending(x => x.score)
            .Take(takeLimit)
            .ToList();
    }

    private List<BotMove> GenerateAllMoves(Game game)
    {
        var moves = new List<BotMove>();

        if (game.CurrentPhase == TurnPhase.BobailMoveRequired)
        {
            foreach (var m in game.GetValidBobailMoves())
                moves.Add(BotMove.Bobail(m));
        }
        else
        {
            var pieces = game.Board.Pieces
                .Where(p => !p.IsBobail &&
                            p.Owner == game.CurrentTurn);

            foreach (var piece in pieces)
            {
                var validMoves =
                    game.GetValidPlayerMoves(piece.Position);

                foreach (var m in validMoves)
                    moves.Add(BotMove.Piece(piece.Position, m));
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
}