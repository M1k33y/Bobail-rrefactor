using System.Collections.Concurrent;

namespace Bobail.API.Realtime;

public class InMemoryGameConnectionTracker : IGameConnectionTracker
{
    private readonly ConcurrentDictionary<string, TrackedConnection> _connections = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, byte>> _gameConnections = new();

    public Task TrackConnectionAsync(string connectionId, Guid userId)
    {
        _connections.AddOrUpdate(
            connectionId,
            _ => new TrackedConnection(userId),
            (_, existing) =>
            {
                existing.UserId = userId;
                return existing;
            });

        return Task.CompletedTask;
    }

    public Task TrackGameConnectionAsync(string connectionId, Guid gameId)
    {
        var connection = _connections.GetOrAdd(
            connectionId,
            _ => new TrackedConnection(null));

        connection.GameIds[gameId] = 0;

        var connectionsForGame = _gameConnections.GetOrAdd(
            gameId,
            _ => new ConcurrentDictionary<string, byte>());

        connectionsForGame[connectionId] = 0;

        return Task.CompletedTask;
    }

    public Task RemoveConnectionAsync(string connectionId)
    {
        if (!_connections.TryRemove(connectionId, out var connection))
            return Task.CompletedTask;

        foreach (var gameId in connection.GameIds.Keys)
        {
            if (!_gameConnections.TryGetValue(gameId, out var connectionsForGame))
                continue;

            connectionsForGame.TryRemove(connectionId, out _);

            if (connectionsForGame.IsEmpty)
                _gameConnections.TryRemove(gameId, out _);
        }

        return Task.CompletedTask;
    }

    public IReadOnlyCollection<string> GetConnectionsForGame(Guid gameId)
    {
        if (!_gameConnections.TryGetValue(gameId, out var connections))
            return Array.Empty<string>();

        return connections.Keys.ToList();
    }

    private sealed class TrackedConnection
    {
        public TrackedConnection(Guid? userId)
        {
            UserId = userId;
        }

        public Guid? UserId { get; set; }
        public ConcurrentDictionary<Guid, byte> GameIds { get; } = new();
    }
}
