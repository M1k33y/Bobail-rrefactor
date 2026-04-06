using Bobail.Application.DTOs;

namespace Bobail.Application.Interfaces.Repositories;

public interface IGameHistoryRepository
{
    Task<List<GameHistoryResponse>> GetHistoryForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<GameReplayResponse?> GetReplayAsync(Guid gameId, Guid userId, CancellationToken cancellationToken = default);
}
