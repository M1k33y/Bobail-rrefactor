using Bobail.Application.Interfaces.Repositories;
using Bobail.Domain.Games;
using Microsoft.EntityFrameworkCore;

namespace Bobail.Infrastructure.Persistence;

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
            StateJson = GameSerializer.Serialize(game)
        };

        _context.Games.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[SQL] GetById: {id}");

        var entity = await _context.Games
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity == null)
            return null;

        var game = GameSerializer.Deserialize(entity.StateJson);

        var piece = game.Board.Pieces
            .FirstOrDefault(p => p.Position.Row == 3 && p.Position.Column == 0);

        if (piece != null)
        {
            Console.WriteLine($"CHECK AFTER LOAD: {piece.Position.Row},{piece.Position.Column}");
        }
        else
        {
            Console.WriteLine("Piece not found at 3,0");
        }

        Console.WriteLine($"[AFTER DESERIALIZE] Id: {game.Id}");

        return game;
    }

    public async Task UpdateAsync(Game game, CancellationToken cancellationToken = default)
    {

        var entity = await _context.Games
            .FirstOrDefaultAsync(x => x.Id == game.Id, cancellationToken);

        if (entity == null)
            throw new InvalidOperationException("Game not found.");

        entity.StateJson = GameSerializer.Serialize(game);

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