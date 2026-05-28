using Bobail.API.Hubs;
using Bobail.Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace Bobail.API.Realtime;

public sealed class OnlineGameTimeoutHostedService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<GameHub> _gameHub;
    private readonly ILogger<OnlineGameTimeoutHostedService> _logger;

    public OnlineGameTimeoutHostedService(
        IServiceScopeFactory scopeFactory,
        IHubContext<GameHub> gameHub,
        ILogger<OnlineGameTimeoutHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _gameHub = gameHub;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireTimedOutGamesAsync(stoppingToken);
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while expiring timed-out online games.");
                await Task.Delay(PollInterval, stoppingToken);
            }
        }
    }

    private async Task ExpireTimedOutGamesAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var onlineGameService = scope.ServiceProvider.GetRequiredService<IOnlineGameService>();
        var expiredGames = await onlineGameService.ExpireTimedOutGamesAsync(stoppingToken);

        foreach (var game in expiredGames)
        {
            await _gameHub.Clients
                .Group(game.Id.ToString("D"))
                .SendAsync("GameEnded", game, stoppingToken);
        }
    }
}
