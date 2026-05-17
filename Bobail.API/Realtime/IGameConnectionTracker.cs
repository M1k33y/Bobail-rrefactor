namespace Bobail.API.Realtime;

public interface IGameConnectionTracker
{
    Task TrackConnectionAsync(string connectionId, Guid userId);

    Task TrackGameConnectionAsync(string connectionId, Guid gameId);

    Task RemoveConnectionAsync(string connectionId);

    IReadOnlyCollection<string> GetConnectionsForGame(Guid gameId);
}
