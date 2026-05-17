using Bobail.Domain.Games;

namespace Bobail.Application.Interfaces.Repositories;

public interface IGamePlayerRepository
{
    Task AddPlayersForGame(Guid gameId, Guid userId, bool isVsBot, PlayerColor? botColor = null);
    Task<bool> UserParticipatesInGameAsync(
        Guid gameId,
        Guid userId,
        CancellationToken cancellationToken = default);

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
