using Bobail.Domain.Common;

namespace Bobail.Domain.Games;

public sealed class Turn : ValueObject
{
    public Move? BobailMove { get; }
    public Move PlayerMove { get; }

    public Turn(Move? bobailMove, Move playerMove)
    {
        PlayerMove = playerMove
            ?? throw new ArgumentNullException(nameof(playerMove));

        BobailMove = bobailMove;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return BobailMove ?? new object();
        yield return PlayerMove;
    }
}
