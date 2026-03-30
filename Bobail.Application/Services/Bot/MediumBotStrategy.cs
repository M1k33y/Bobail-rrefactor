using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;

namespace Bobail.Infrastructure.Bots;

public class MediumBotStrategy : IBotStrategy
{
    private readonly MediumBoardEvaluator _evaluator;
    public BotDifficulty Difficulty => BotDifficulty.Medium;

    public MediumBotStrategy(
    MediumBoardEvaluator evaluator)
    {
        _evaluator = evaluator;
    }

    public BotMove DecideMove(Game game)
    {
        var possibleMoves = GenerateAllMoves(game);

        if (possibleMoves.Count == 0)
            throw new InvalidOperationException("Bot has no valid moves.");

        int bestScore = int.MinValue;
        BotMove bestMove = possibleMoves[0];

        foreach (var move in possibleMoves)
        {
            var clone = game.Clone();

            ApplyMove(clone, move);

            int score = _evaluator.Evaluate(clone, game.CurrentTurn);

            if (AllowsImmediateOpponentWin(clone, game.CurrentTurn))
                score -= 80000;

            if (CanWinNextTurn(clone, game.CurrentTurn))
                score += 60000;
           
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private bool AllowsImmediateOpponentWin(Game gameAfterBotMove, PlayerColor botColor)
    {
        if (gameAfterBotMove.Status == GameStatus.Finished)
            return false;

        var opponent = botColor == PlayerColor.Red
            ? PlayerColor.Green
            : PlayerColor.Red;

    
        if (gameAfterBotMove.CurrentTurn != opponent)
            return false;

        
        if (gameAfterBotMove.CurrentPhase == TurnPhase.BobailMoveRequired)
        {
            var bobailMoves = gameAfterBotMove.GetValidBobailMoves();

            foreach (var move in bobailMoves)
            {
                var clone = gameAfterBotMove.Clone();
                clone.ExecuteBobailMove(move);

                if (clone.Status == GameStatus.Finished &&
                    clone.Winner == opponent)
                {
                    return true;
                }
            }
        }
        else
        {
            var pieces = gameAfterBotMove.Board.Pieces
                .Where(p => !p.IsBobail &&
                            p.Owner == opponent);

            foreach (var piece in pieces)
            {
                var moves = gameAfterBotMove.GetValidPlayerMoves(piece.Position);

                foreach (var move in moves)
                {
                    var clone = gameAfterBotMove.Clone();
                    clone.ExecutePlayerMove(piece.Position, move);

                    if (clone.Status == GameStatus.Finished &&
                        clone.Winner == opponent)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
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
                            p.Owner == game.CurrentTurn)
                .ToList();

            foreach (var piece in pieces)
            {
                var validMoves = game.GetValidPlayerMoves(piece.Position);

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

    private bool CanWinNextTurn(Game gameAfterMove, PlayerColor botColor)
    {
        if (gameAfterMove.Status == GameStatus.Finished)
            return false;

        if (gameAfterMove.CurrentTurn != botColor)
            return false;

        if (gameAfterMove.CurrentPhase == TurnPhase.BobailMoveRequired)
        {
            var bobailMoves = gameAfterMove.GetValidBobailMoves();

            foreach (var move in bobailMoves)
            {
                var clone = gameAfterMove.Clone();
                clone.ExecuteBobailMove(move);

                if (clone.Status == GameStatus.Finished &&
                    clone.Winner == botColor)
                    return true;
            }
        }
        else
        {
            var pieces = gameAfterMove.Board.Pieces
                .Where(p => !p.IsBobail &&
                            p.Owner == botColor);

            foreach (var piece in pieces)
            {
                var moves = gameAfterMove.GetValidPlayerMoves(piece.Position);

                foreach (var move in moves)
                {
                    var clone = gameAfterMove.Clone();
                    clone.ExecutePlayerMove(piece.Position, move);

                    if (clone.Status == GameStatus.Finished &&
                        clone.Winner == botColor)
                        return true;
                }
            }
        }

        return false;
    }

}