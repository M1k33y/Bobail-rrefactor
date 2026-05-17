namespace Bobail.Application.Interfaces.Services;

public interface IGameLockManager
{
    Task<IDisposable> AcquireAsync(
        Guid gameId,
        CancellationToken cancellationToken = default);
}
