using System.Collections.Concurrent;
using Bobail.Application.Interfaces.Services;

namespace Bobail.Application.Services;

public class InMemoryGameLockManager : IGameLockManager
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    public async Task<IDisposable> AcquireAsync(
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        var semaphore = _locks.GetOrAdd(gameId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);

        return new LockHandle(semaphore);
    }

    private sealed class LockHandle : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        public LockHandle(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _semaphore.Release();
        }
    }
}
