using Bobail.Application.Interfaces.Services;
using Bobail.Domain.Games;

namespace Bobail.Application.Services;

public class OnlineGameClockService : IOnlineGameClockService
{
    public TimeSpan DefaultInitialTime { get; } = TimeSpan.FromMinutes(3);

    public void StartClock(Game game, DateTimeOffset nowUtc)
    {
        game.StartClock(
            TimeControl.Create(DefaultInitialTime),
            nowUtc);
    }

    public PlayerColor? GetTimedOutPlayer(Game game, DateTimeOffset nowUtc)
    {
        if (game is not { Mode: GameMode.OnlineMultiplayer, Status: GameStatus.InProgress } ||
            game.Clock is null)
        {
            return null;
        }

        var remaining = game.Clock.GetRemainingMilliseconds(
            game.CurrentTurn,
            game.CurrentTurn,
            nowUtc);

        return remaining <= 0
            ? game.CurrentTurn
            : null;
    }

    public bool FinishIfTimedOut(Game game, DateTimeOffset nowUtc)
    {
        var timedOutPlayer = GetTimedOutPlayer(game, nowUtc);

        if (!timedOutPlayer.HasValue)
            return false;

        game.Clock!.Expire(timedOutPlayer.Value);
        game.Finish(
            GetOpponentColor(timedOutPlayer.Value),
            GameEndReason.Timeout);
        return true;
    }

    public void CommitSuccessfulMove(
        Game game,
        PlayerColor movingPlayer,
        DateTimeOffset nowUtc)
    {
        if (game.Clock is null)
            return;

        game.Clock.CommitElapsed(movingPlayer, nowUtc);

        if (game.Status == GameStatus.InProgress)
            return;

        game.Clock.Stop();
    }

    private static PlayerColor GetOpponentColor(PlayerColor playerColor)
    {
        return playerColor == PlayerColor.Red
            ? PlayerColor.Green
            : PlayerColor.Red;
    }
}
