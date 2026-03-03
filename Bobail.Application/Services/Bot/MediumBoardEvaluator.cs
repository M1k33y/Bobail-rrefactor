using Bobail.Application.Interfaces.Services;
using Bobail.Domain.Games;
using Bobail.Domain.Board;

public class MediumBoardEvaluator : IBoardEvaluator
{
    private const int WinScore = 100000;
    private const int LossScore = -100000;

    public int Evaluate(Game game, PlayerColor botColor)
    {
        // win/loss check
        if (game.Status == GameStatus.Finished)
        {
            if (game.Winner == botColor)
                return WinScore;

            return LossScore;
        }

        int score = 0;

        score += EvaluateBobailProgress(game, botColor);
        score += EvaluateBobailMobility(game);
        score += EvaluatePotentialBlock(game, botColor);

        return score;
    }

    private int EvaluateBobailProgress(Game game, PlayerColor botColor)
    {
        var bobail = game.Board.Pieces
            .First(p => p.IsBobail);

        int boardSize = 5; 

        int targetRow = botColor == PlayerColor.Red
            ? 0
            : boardSize - 1;

        int distance = Math.Abs(bobail.Position.Row - targetRow);

        int maxDistance = boardSize - 1;

        int progressScore = (maxDistance - distance) * 200;

        return progressScore;
    }

   
    private int EvaluateBobailMobility(Game game)
    {
        var moves = game.GetValidBobailMoves();

        int mobilityPenalty = moves.Count * -50;

        return mobilityPenalty;
    }


    private int EvaluatePotentialBlock(Game game, PlayerColor botColor)
    {
        var moves = game.GetValidBobailMoves();


        if (moves.Count == 0)
        {
            if (game.CurrentTurn != botColor)
                return 50000; // bot a blocat adversarul

            return -50000; // bot s-a auto-blocat
        }

        return 0;
    }

    
}