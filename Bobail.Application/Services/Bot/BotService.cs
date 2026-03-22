
using Bobail.Application.Interfaces.Services;
using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;
using Microsoft.Extensions.Logging;

namespace Bobail.Application.Services;

public class BotService : IBotService
{
    private readonly IEnumerable<IBotStrategy> _strategies;
    private readonly ILogger<BotService> _logger;

    public BotService(
        IEnumerable<IBotStrategy> strategies,
        ILogger<BotService> logger)
    {
        _strategies = strategies;
        _logger = logger;
    }

    public Task ExecuteSingleMoveAsync(Game game)
    {
        Console.WriteLine($"BOT MOVE START → Turn: {game.CurrentTurn}, Phase: {game.CurrentPhase}");

        var moves = game.GetValidBobailMoves(); 
        Console.WriteLine($"BOT: found {moves.Count} moves");

        if (!game.BotDifficulty.HasValue)
            throw new InvalidOperationException("Bot difficulty not set.");

        var strategy = _strategies
            .First(s => s.Difficulty == game.BotDifficulty.Value);

        _logger.LogInformation(
           "Bot move executing. Difficulty: {Difficulty}, Strategy: {Strategy}, Turn: {Turn}",
           game.BotDifficulty,
           strategy.GetType().Name,
           game.CurrentTurn);

        var move = strategy.DecideMove(game);

        if (move.IsBobailMove)
            game.ExecuteBobailMove(move.To);
        else
            game.ExecutePlayerMove(move.From, move.To);
        Console.WriteLine($"BOT MOVE END → Turn: {game.CurrentTurn}, Phase: {game.CurrentPhase}");
        return Task.CompletedTask;
    }
}