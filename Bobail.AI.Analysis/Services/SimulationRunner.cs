using Bobail.AI.Analysis.Models;
using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;

namespace Bobail.AI.Analysis.Services;

public sealed class SimulationRunner
{
    private readonly BotFactory _botFactory;
    private readonly int _maxTurnsPerGame;

    public SimulationRunner(BotFactory botFactory, int maxTurnsPerGame)
    {
        _botFactory = botFactory;
        _maxTurnsPerGame = maxTurnsPerGame;
    }

    public IReadOnlyList<MatchGameResult> RunMatchup(MatchupDefinition matchup, int gamesToPlay)
    {
        var results = new MatchGameResult[gamesToPlay];

        Parallel.For(0, gamesToPlay, gameIndex =>
        {
            bool botAStarts = gameIndex % 2 == 0;

            var redProfile = botAStarts ? matchup.BotA : matchup.BotB;
            var greenProfile = botAStarts ? matchup.BotB : matchup.BotA;

            var redBot = _botFactory.Create(redProfile);
            var greenBot = _botFactory.Create(greenProfile);

            results[gameIndex] = PlaySingleGame(
                matchup.BotAName,
                matchup.BotBName,
                redProfile.Name,
                redBot,
                greenBot,
                redProfile,
                greenProfile);
        });

        return results;
    }

    private MatchGameResult PlaySingleGame(
        string botAName,
        string botBName,
        string startingBotName,
        IBotStrategy redBot,
        IBotStrategy greenBot,
        BotProfile redProfile,
        BotProfile greenProfile)
    {
        var game = new Game(GameMode.LocalMultiplayer);
        int turns = 0;

        while (game.Status == GameStatus.InProgress && turns < _maxTurnsPerGame)
        {
            if (!HasAnyValidMove(game))
            {
                string winner = ResolveBotName(redProfile, greenProfile, Opponent(game.CurrentTurn));
                return new MatchGameResult(botAName, botBName, startingBotName, winner, turns, false);
            }

            var activeBot = game.CurrentTurn == PlayerColor.Red ? redBot : greenBot;
            var move = activeBot.DecideMove(game);

            if (move.IsBobailMove)
                game.ExecuteBobailMove(move.To);
            else
                game.ExecutePlayerMove(move.From, move.To);

            turns++;
        }

        var winnerName = game.Winner is null
            ? null
            : ResolveBotName(redProfile, greenProfile, game.Winner.Value);

        return new MatchGameResult(
            botAName,
            botBName,
            startingBotName,
            winnerName,
            turns,
            game.Status == GameStatus.InProgress);
    }

    private static bool HasAnyValidMove(Game game)
    {
        if (game.CurrentPhase == TurnPhase.BobailMoveRequired)
            return game.GetValidBobailMoves().Count > 0;

        return game.Board.Pieces
            .Where(piece => !piece.IsBobail && piece.Owner == game.CurrentTurn)
            .Any(piece => game.GetValidPlayerMoves(piece.Position).Count > 0);
    }

    private static string ResolveBotName(
        BotProfile redProfile,
        BotProfile greenProfile,
        PlayerColor winner)
    {
        return winner == PlayerColor.Red
            ? redProfile.Name
            : greenProfile.Name;
    }

    private static PlayerColor Opponent(PlayerColor color)
    {
        return color == PlayerColor.Red ? PlayerColor.Green : PlayerColor.Red;
    }
}
