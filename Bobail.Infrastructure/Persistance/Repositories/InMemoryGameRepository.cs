using Bobail.Application.Interfaces.Repositories;
using Bobail.Domain.Games;
using System.Collections.Concurrent;

namespace Bobail.Infrastructure.Persistance.Repositories;

public class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<Guid, Game> _storage = new();

    public Task AddAsync(Game game, CancellationToken cancellationToken = default)
    {
        if (!_storage.TryAdd(game.Id, game))
            throw new InvalidOperationException("Game already exists.");

        return Task.CompletedTask;
    }

    public Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _storage.TryGetValue(id, out var game);
        return Task.FromResult(game);
    }

    public Task UpdateAsync(Game game, CancellationToken cancellationToken = default)
    {
        if (!_storage.ContainsKey(game.Id))
            throw new InvalidOperationException("Game does not exist.");

        _storage[game.Id] = game;

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage.ContainsKey(id));
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_storage.TryRemove(id, out _))
            throw new InvalidOperationException("Game does not exist.");

        return Task.CompletedTask;
    }
}
