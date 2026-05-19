using Bobail.Application.Interfaces.Repositories;
using Bobail.Application.DTOs;
using Bobail.Domain.Games;
using Bobail.Infrastructure.Persistance;
using Bobail.Infrastructure.Persistance.Entities;
using Bobail.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bobail.Infrastructure.Persistance.Repositories;

public class SqlGameStateRepository : IGameStateRepository
{
    private readonly GameDbContext _context;

    public SqlGameStateRepository(GameDbContext context)
    {
        _context = context;
    }

    public async Task AddSnapshotAsync(Game game, CancellationToken cancellationToken = default)
    {
        var lastMoveNumber = await _context.GameStates
            .Where(x => x.GameId == game.Id)
            .Select(x => (int?)x.MoveNumber)
            .MaxAsync(cancellationToken) ?? -1;

        var entity = new GameStateEntity
        {
            Id = Guid.NewGuid(),
            GameId = game.Id,
            MoveNumber = lastMoveNumber + 1,
            StateJson = GameSerializer.Serialize(game),
            CreatedAt = DateTime.UtcNow
        };

        _context.GameStates.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<List<GameStateSnapshotRecord>> GetByGameIdAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        return _context.GameStates
            .Where(x => x.GameId == gameId)
            .OrderBy(x => x.MoveNumber)
            .Select(x => new GameStateSnapshotRecord
            {
                MoveNumber = x.MoveNumber,
                StateJson = x.StateJson,
                CreatedAtUtc = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
