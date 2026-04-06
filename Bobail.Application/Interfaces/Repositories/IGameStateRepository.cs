using Bobail.Application.DTOs;
using Bobail.Domain.Games;

namespace Bobail.Application.Interfaces.Repositories;

public interface IGameStateRepository
{
    Task AddSnapshotAsync(Game game, CancellationToken cancellationToken = default);
    Task<List<GameStateSnapshotRecord>> GetByGameIdAsync(Guid gameId, CancellationToken cancellationToken = default);
}
