using Bobail.Application.DTOs;

namespace Bobail.Application.Interfaces.Repositories;

public interface IGameHistoryRepository
{
    Task<PagedGameHistoryResponse> GetHistoryForUserAsync(
        Guid userId,
        GameHistoryQuery query,
        CancellationToken cancellationToken = default);

    Task<UserGameStatsResponse?> GetUserStatsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<GameReplayResponse?> GetReplayAsync(Guid gameId, Guid userId, CancellationToken cancellationToken = default);
}
