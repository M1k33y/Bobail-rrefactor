using Bobail.Domain.Games;

namespace Bobail.Application.Interfaces.Repositories;

public interface IGamePlayerRepository
{
    Task AddPlayersForGame(Guid gameId, Guid userId, bool isVsBot, PlayerColor? botColor = null);
    Task<bool> UserParticipatesInGameAsync(
        Guid gameId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetActiveOnlineGameIdsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    async Task<Guid?> GetActiveOnlineGameIdForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var gameIds = await GetActiveOnlineGameIdsForUserAsync(userId, cancellationToken);
        return gameIds.Count == 0 ? null : gameIds[0];
    }

    Task<PlayerColor?> GetPlayerColorAsync(
        Guid gameId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<PlayerColor> AddOnlinePlayerAsync(
        Guid gameId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<int> CountHumanPlayersAsync(
        Guid gameId,
        CancellationToken cancellationToken = default);
}
