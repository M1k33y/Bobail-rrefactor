using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;
using Bobail.Domain.Pieces;
using FluentAssertions;

namespace Bobail.Application.Tests.Services.Bot;

public class EasyBotStrategyTests
{
    [Fact]
    public void DecideMove_During_Player_Phase_Returns_Valid_Player_Move()
    {
        var game = new Game();
        var strategy = new EasyBotStrategy();

        var move = strategy.DecideMove(game);

        move.IsBobailMove.Should().BeFalse();
        game.Board.GetPieceAt(move.From)!.Owner.Should().Be(PlayerColor.Red);
        game.GetValidPlayerMoves(move.From).Should().Contain(move.To);
    }

    [Fact]
    public void DecideMove_During_Green_Bobail_Phase_Prefers_Forward_Row()
    {
        var game = new Game();
        game.ExecutePlayerMove(P(0, 0), P(3, 0));
        var strategy = new EasyBotStrategy();

        var move = strategy.DecideMove(game);

        move.IsBobailMove.Should().BeTrue();
        move.To.Row.Should().Be(3);
        game.GetValidBobailMoves().Should().Contain(move.To);
    }

    [Fact]
    public void DecideMove_When_No_Player_Moves_Exist_Throws()
    {
        var game = new Game();
        game.Board.Pieces.RemoveAll(piece =>
            !piece.IsBobail && piece.Owner == PlayerColor.Red);
        var strategy = new EasyBotStrategy();

        var act = () => strategy.DecideMove(game);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Easy bot has no valid player moves.");
    }

    [Fact]
    public void DecideMove_When_No_Bobail_Moves_Exist_Throws()
    {
        var game = new Game();
        game.ExecutePlayerMove(P(0, 0), P(3, 0));
        SurroundBobail(game);
        var strategy = new EasyBotStrategy();

        var act = () => strategy.DecideMove(game);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Easy bot has no valid Bobail moves.");
    }

    private static void SurroundBobail(Game game)
    {
        game.Board.Pieces.Clear();
        game.Board.Pieces.Add(new Piece(PieceType.Bobail, P(2, 2)));
        game.Board.Pieces.AddRange(
        [
            PlayerPiece(PlayerColor.Red, 1, 1),
            PlayerPiece(PlayerColor.Red, 1, 2),
            PlayerPiece(PlayerColor.Red, 1, 3),
            PlayerPiece(PlayerColor.Red, 2, 1),
            PlayerPiece(PlayerColor.Red, 2, 3),
            PlayerPiece(PlayerColor.Green, 3, 1),
            PlayerPiece(PlayerColor.Green, 3, 2),
            PlayerPiece(PlayerColor.Green, 3, 3)
        ]);
    }

    private static Piece PlayerPiece(PlayerColor owner, int row, int column)
    {
        return new Piece(PieceType.PlayerPiece, P(row, column), owner);
    }

    private static Position P(int row, int column)
    {
        return new Position(row, column);
    }
}
