using Bobail.Domain.Games;

namespace Bobail.Application.Interfaces.Services;

public interface IOnlineGameClockService
{
    TimeSpan DefaultInitialTime { get; }

    void StartClock(Game game, DateTimeOffset nowUtc);

    PlayerColor? GetTimedOutPlayer(Game game, DateTimeOffset nowUtc);

    bool FinishIfTimedOut(Game game, DateTimeOffset nowUtc);

    void CommitSuccessfulMove(Game game, PlayerColor movingPlayer, DateTimeOffset nowUtc);
}
