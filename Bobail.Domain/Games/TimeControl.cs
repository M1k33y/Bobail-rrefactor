using Bobail.Domain.Common;
using System.Text.Json.Serialization;

namespace Bobail.Domain.Games;

public sealed class TimeControl
{
    [JsonConstructor]
    private TimeControl(long initialTimeMilliseconds)
    {
        if (initialTimeMilliseconds <= 0)
            throw new DomainException("Initial clock time must be positive.");

        InitialTimeMilliseconds = initialTimeMilliseconds;
    }

    public long InitialTimeMilliseconds { get; }

    public static TimeControl Create(TimeSpan initialTime)
    {
        return new TimeControl(ToMilliseconds(initialTime));
    }

    private static long ToMilliseconds(TimeSpan time)
    {
        if (time <= TimeSpan.Zero)
            throw new DomainException("Initial clock time must be positive.");

        return checked((long)Math.Ceiling(time.TotalMilliseconds));
    }
}
