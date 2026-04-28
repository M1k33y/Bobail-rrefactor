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

            var redDifficulty = botAStarts ? matchup.BotA : matchup.BotB;
            var greenDifficulty = botAStarts ? matchup.BotB : matchup.BotA;

            var redBot = _botFactory.Create(redDifficulty);
            var greenBot = _botFactory.Create(greenDifficulty);

            results[gameIndex] = PlaySingleGame(
                matchup.BotAName,
                matchup.BotBName,
                botAStarts ? matchup.BotAName : matchup.BotBName,
                redBot,
                greenBot,
                redDifficulty,
                greenDifficulty);
        });

        return results;
    }

    private MatchGameResult PlaySingleGame(
        string botAName,
        string botBName,
        string startingBotName,
        IBotStrategy redBot,
        IBotStrategy greenBot,
        BotDifficulty redDifficulty,
        BotDifficulty greenDifficulty)
    {
        var game = new Game(GameMode.LocalMultiplayer);
        int turns = 0;

        while (game.Status == GameStatus.InProgress && turns < _maxTurnsPerGame)
        {
            if (!HasAnyValidMove(game))
            {
                string winner = ResolveBotName(botAName, botBName, redDifficulty, greenDifficulty, Opponent(game.CurrentTurn));
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
            : ResolveBotName(botAName, botBName, redDifficulty, greenDifficulty, game.Winner.Value);

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
        string botAName,
        string botBName,
        BotDifficulty redDifficulty,
        BotDifficulty greenDifficulty,
        PlayerColor winner)
    {
        var winningDifficulty = winner == PlayerColor.Red ? redDifficulty : greenDifficulty;
        return winningDifficulty.ToString() == botAName ? botAName : botBName;
    }

    private static PlayerColor Opponent(PlayerColor color)
    {
        return color == PlayerColor.Red ? PlayerColor.Green : PlayerColor.Red;
    }
}
