using Bobail.Domain.Board;
using Bobail.Domain.Games;
using FluentAssertions;

namespace Bobail.Domain.Tests.Board;

public class BoardTests
{
    [Fact]
    public void New_Board_Starts_With_Five_Red_Five_Green_And_Bobail()
    {
        var board = new Bobail.Domain.Board.Board();

        board.Pieces.Should().HaveCount(11);
        board.Pieces.Where(piece => piece.Owner == PlayerColor.Red).Should().HaveCount(5);
        board.Pieces.Where(piece => piece.Owner == PlayerColor.Green).Should().HaveCount(5);
        board.Pieces.Single(piece => piece.IsBobail).Position.Should().Be(P(2, 2));
        board.Pieces.Where(piece => piece.Owner == PlayerColor.Red)
            .Should().OnlyContain(piece => piece.Position.Row == 0);
        board.Pieces.Where(piece => piece.Owner == PlayerColor.Green)
            .Should().OnlyContain(piece => piece.Position.Row == 4);
    }

    [Fact]
    public void MovePiece_Updates_Piece_Position()
    {
        var board = new Bobail.Domain.Board.Board();
        var piece = board.GetPieceAt(P(0, 0));

        board.MovePiece(piece!, P(3, 0));

        piece!.Position.Should().Be(P(3, 0));
        board.GetPieceAt(P(0, 0)).Should().BeNull();
        board.GetPieceAt(P(3, 0)).Should().BeSameAs(piece);
    }

    [Fact]
    public void Clone_Creates_Independent_Pieces()
    {
        var board = new Bobail.Domain.Board.Board();
        var clone = board.Clone();
        var clonedBobail = clone.Pieces.Single(piece => piece.IsBobail);

        clone.MovePiece(clonedBobail, P(1, 1));

        board.Pieces.Single(piece => piece.IsBobail).Position.Should().Be(P(2, 2));
        clonedBobail.Position.Should().Be(P(1, 1));
    }

    private static Position P(int row, int column)
    {
        return new Position(row, column);
    }
}
