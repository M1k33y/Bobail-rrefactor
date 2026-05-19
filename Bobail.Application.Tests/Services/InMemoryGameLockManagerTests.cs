using Bobail.Application.Services;
using FluentAssertions;

namespace Bobail.Application.Tests.Services;

public class InMemoryGameLockManagerTests
{
    [Fact]
    public async Task AcquireAsync_For_Same_Game_Waits_Until_First_Lock_Is_Disposed()
    {
        var manager = new InMemoryGameLockManager();
        var gameId = Guid.NewGuid();
        using var firstLock = await manager.AcquireAsync(gameId);

        var secondLockTask = manager.AcquireAsync(gameId);
        var completedBeforeRelease = await Task.WhenAny(
            secondLockTask,
            Task.Delay(50)) == secondLockTask;

        completedBeforeRelease.Should().BeFalse();

        firstLock.Dispose();
        using var secondLock = await secondLockTask.WaitAsync(TimeSpan.FromSeconds(1));

        secondLock.Should().NotBeNull();
    }

    [Fact]
    public async Task AcquireAsync_For_Different_Games_Does_Not_Block()
    {
        var manager = new InMemoryGameLockManager();
        using var firstLock = await manager.AcquireAsync(Guid.NewGuid());

        using var secondLock = await manager
            .AcquireAsync(Guid.NewGuid())
            .WaitAsync(TimeSpan.FromSeconds(1));

        secondLock.Should().NotBeNull();
    }

    [Fact]
    public async Task Disposing_Lock_Twice_Is_Safe()
    {
        var manager = new InMemoryGameLockManager();
        var gameId = Guid.NewGuid();
        var gameLock = await manager.AcquireAsync(gameId);

        gameLock.Dispose();
        var act = () => gameLock.Dispose();

        act.Should().NotThrow();

        using var nextLock = await manager
            .AcquireAsync(gameId)
            .WaitAsync(TimeSpan.FromSeconds(1));
    }
}
