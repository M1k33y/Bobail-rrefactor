using Bobail.Domain.Board;
using Bobail.Domain.Common;

namespace Bobail.Domain.Games;

public sealed class Move : ValueObject
{
    public Position From { get; }
    public Position To { get; }

    public Move(Position from, Position to)
    {
        From = from ?? throw new ArgumentNullException(nameof(from));
        To = to ?? throw new ArgumentNullException(nameof(to));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return From;
        yield return To;
    }
}
