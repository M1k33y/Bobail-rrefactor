using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;

namespace Bobail.Training.Simulation;

public sealed class GameSimulator
{
    public GameResult PlayGame(IBotStrategy redBot, IBotStrategy greenBot, int maxTurns)
    {
        var game = new Game(GameMode.LocalMultiplayer);
        int turns = 0;

        while (game.Status == GameStatus.InProgress && turns < maxTurns)
        {
            if (!HasAnyValidMove(game))
            {
                return new GameResult(Opponent(game.CurrentTurn), turns, false);
            }

            var activeBot = game.CurrentTurn == PlayerColor.Red ? redBot : greenBot;
            var move = activeBot.DecideMove(game);

            if (move.IsBobailMove)
                game.ExecuteBobailMove(move.To);
            else
                game.ExecutePlayerMove(move.From, move.To);

            turns++;
        }

        return new GameResult(game.Winner, turns, game.Status == GameStatus.InProgress);
    }

    private static bool HasAnyValidMove(Game game)
    {
        if (game.CurrentPhase == TurnPhase.BobailMoveRequired)
            return game.GetValidBobailMoves().Count > 0;

        return game.Board.Pieces
            .Where(p => !p.IsBobail && p.Owner == game.CurrentTurn)
            .Any(piece => game.GetValidPlayerMoves(piece.Position).Count > 0);
    }

    private static PlayerColor Opponent(PlayerColor color)
    {
        return color == PlayerColor.Red
            ? PlayerColor.Green
            : PlayerColor.Red;
    }
}
