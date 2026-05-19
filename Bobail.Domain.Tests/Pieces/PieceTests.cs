using Bobail.Domain.Common;
using Bobail.Domain.Games;
using Bobail.Domain.Pieces;
using FluentAssertions;

namespace Bobail.Domain.Tests.Pieces;

public class PieceTests
{
    [Fact]
    public void Player_Piece_Requires_Owner()
    {
        var act = () => new Piece(PieceType.PlayerPiece, P(0, 0));

        act.Should()
            .Throw<DomainException>()
            .WithMessage("PlayerPiece must have an owner.");
    }

    [Fact]
    public void Bobail_Cannot_Have_Owner()
    {
        var act = () => new Piece(PieceType.Bobail, P(2, 2), PlayerColor.Red);

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Bobail cannot have an owner.");
    }

    private static Position P(int row, int column)
    {
        return new Position(row, column);
    }
}
