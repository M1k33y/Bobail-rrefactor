using Bobail.Application.Interfaces.Repositories;
using Bobail.Domain.Games;
using Microsoft.EntityFrameworkCore;
using Bobail.Infrastructure.Persistance;
using Bobail.Infrastructure.Persistance.Entities;
using Bobail.Infrastructure.Persistence;

namespace Bobail.Infrastructure.Persistance.Repositories;

public class SqlGameRepository : IGameRepository
{
    private readonly GameDbContext _context;

    public SqlGameRepository(GameDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Game game, CancellationToken cancellationToken = default)
    {
        var entity = new GameEntity
        {
            Id = game.Id,
            StateJson = GameSerializer.Serialize(game),

            Status = (int)game.Status,
            CurrentTurn = (int)game.CurrentTurn,
            Mode = (int)game.Mode,
            BotDifficulty = game.BotDifficulty.HasValue ? (int)game.BotDifficulty.Value : null,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Games.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {

        var entity = await _context.Games
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity == null)
            return null;

        var game = GameSerializer.Deserialize(entity.StateJson);

        return game;
    }

    public async Task<IReadOnlyList<Guid>> GetInProgressOnlineGameIdsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Games
            .Where(x =>
                x.Mode == (int)GameMode.OnlineMultiplayer &&
                x.Status == (int)GameStatus.InProgress)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Game game, CancellationToken cancellationToken = default)
    {

        var entity = await _context.Games
            .FirstOrDefaultAsync(x => x.Id == game.Id, cancellationToken);

        if (entity == null)
            throw new InvalidOperationException("Game not found.");

        entity.StateJson = GameSerializer.Serialize(game);

        entity.Status = (int)game.Status;
        entity.CurrentTurn = (int)game.CurrentTurn;
        entity.Mode = (int)game.Mode;
        entity.BotDifficulty = game.BotDifficulty.HasValue ? (int)game.BotDifficulty.Value : null;

        entity.UpdatedAt = DateTime.UtcNow;

        if (game.Status == GameStatus.Finished && game.Winner != null)
        {
            var winnerColor = (int)game.Winner;

            var winnerPlayer = await _context.GamePlayers
                .FirstOrDefaultAsync(x =>
                    x.GameId == game.Id &&
                    x.Color == winnerColor);

            if (winnerPlayer != null)
            {
                entity.WinnerUserId = winnerPlayer.UserId;
            }
        }


        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Games
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity == null)
            throw new InvalidOperationException("Game not found.");

        _context.Games.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Games
            .AnyAsync(x => x.Id == id, cancellationToken);
    }
}
