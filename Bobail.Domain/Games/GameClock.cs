using Bobail.Domain.Common;
using System.Text.Json.Serialization;

namespace Bobail.Domain.Games;

public sealed class GameClock
{
    [JsonConstructor]
    private GameClock(
        TimeControl timeControl,
        long redRemainingMilliseconds,
        long greenRemainingMilliseconds,
        DateTimeOffset? turnStartedAtUtc)
    {
        TimeControl = timeControl;
        RedRemainingMilliseconds = Math.Max(0, redRemainingMilliseconds);
        GreenRemainingMilliseconds = Math.Max(0, greenRemainingMilliseconds);
        TurnStartedAtUtc = turnStartedAtUtc;
    }

    public TimeControl TimeControl { get; }
    public long RedRemainingMilliseconds { get; private set; }
    public long GreenRemainingMilliseconds { get; private set; }
    public DateTimeOffset? TurnStartedAtUtc { get; private set; }

    public static GameClock Start(TimeControl timeControl, DateTimeOffset startedAtUtc)
    {
        return new GameClock(
            timeControl,
            timeControl.InitialTimeMilliseconds,
            timeControl.InitialTimeMilliseconds,
            NormalizeUtc(startedAtUtc));
    }

    public long GetRemainingMilliseconds(
        PlayerColor color,
        PlayerColor? activeColor,
        DateTimeOffset nowUtc)
    {
        var remaining = GetStoredRemainingMilliseconds(color);

        if (TurnStartedAtUtc.HasValue && activeColor == color)
            remaining -= GetElapsedMilliseconds(nowUtc);

        return Math.Max(0, remaining);
    }

    public void CommitElapsed(PlayerColor color, DateTimeOffset nowUtc)
    {
        var remaining = GetRemainingMilliseconds(color, color, nowUtc);
        SetRemainingMilliseconds(color, remaining);
        TurnStartedAtUtc = NormalizeUtc(nowUtc);
    }

    public void Expire(PlayerColor color)
    {
        SetRemainingMilliseconds(color, 0);
        Stop();
    }

    public void Stop()
    {
        TurnStartedAtUtc = null;
    }

    public GameClock Clone()
    {
        return new GameClock(
            TimeControl,
            RedRemainingMilliseconds,
            GreenRemainingMilliseconds,
            TurnStartedAtUtc);
    }

    private long GetStoredRemainingMilliseconds(PlayerColor color)
    {
        return color switch
        {
            PlayerColor.Red => RedRemainingMilliseconds,
            PlayerColor.Green => GreenRemainingMilliseconds,
            _ => throw new DomainException("Unsupported player color.")
        };
    }

    private void SetRemainingMilliseconds(PlayerColor color, long remainingMilliseconds)
    {
        var normalizedRemaining = Math.Max(0, remainingMilliseconds);

        switch (color)
        {
            case PlayerColor.Red:
                RedRemainingMilliseconds = normalizedRemaining;
                break;
            case PlayerColor.Green:
                GreenRemainingMilliseconds = normalizedRemaining;
                break;
            default:
                throw new DomainException("Unsupported player color.");
        }
    }

    private long GetElapsedMilliseconds(DateTimeOffset nowUtc)
    {
        if (!TurnStartedAtUtc.HasValue)
            return 0;

        var elapsed = NormalizeUtc(nowUtc) - TurnStartedAtUtc.Value;
        return Math.Max(0, (long)Math.Floor(elapsed.TotalMilliseconds));
    }

    private static DateTimeOffset NormalizeUtc(DateTimeOffset value)
    {
        return value.ToUniversalTime();
    }
}
