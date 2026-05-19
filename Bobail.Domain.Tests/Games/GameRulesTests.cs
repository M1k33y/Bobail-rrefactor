using Bobail.Domain.Common;
using Bobail.Domain.Games;
using Bobail.Domain.Pieces;
using FluentAssertions;

namespace Bobail.Domain.Tests.Games;

public class GameRulesTests
{
    [Fact]
    public void New_Local_Game_Starts_With_Red_Player_Move()
    {
        var game = new Game();

        game.Status.Should().Be(GameStatus.InProgress);
        game.CurrentTurn.Should().Be(PlayerColor.Red);
        game.CurrentPhase.Should().Be(TurnPhase.PlayerMoveRequired);
        game.IsFirstTurn.Should().BeTrue();
    }

    [Fact]
    public void Bobail_Cannot_Move_On_First_Turn()
    {
        var game = new Game();

        var act = () => game.ExecuteBobailMove(P(2, 1));

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Bobail cannot be moved on first turn.");
    }

    [Fact]
    public void Player_Move_Must_Belong_To_Current_Player()
    {
        var game = new Game();

        var act = () => game.ExecutePlayerMove(P(4, 0), P(1, 0));

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Cannot move opponent's piece.");
    }

    [Fact]
    public void Player_Token_Must_Move_As_Far_As_Possible()
    {
        var game = new Game();

        var act = () => game.ExecutePlayerMove(P(0, 0), P(1, 0));

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Player token must move as far as possible in the chosen direction.");
    }

    [Fact]
    public void Valid_Player_Move_Switches_To_Bobail_Phase_And_Next_Player()
    {
        var game = new Game();

        game.ExecutePlayerMove(P(0, 0), P(3, 0));

        game.CurrentTurn.Should().Be(PlayerColor.Green);
        game.CurrentPhase.Should().Be(TurnPhase.BobailMoveRequired);
        game.IsFirstTurn.Should().BeFalse();
        game.Status.Should().Be(GameStatus.InProgress);
    }

    [Fact]
    public void Bobail_Move_After_Player_Move_Returns_To_Player_Phase()
    {
        var game = new Game();
        game.ExecutePlayerMove(P(0, 0), P(3, 0));

        game.ExecuteBobailMove(P(2, 1));

        game.CurrentTurn.Should().Be(PlayerColor.Green);
        game.CurrentPhase.Should().Be(TurnPhase.PlayerMoveRequired);
        game.Status.Should().Be(GameStatus.InProgress);
    }

    [Fact]
    public void Bobail_Can_Only_Move_One_Square()
    {
        var game = new Game();
        game.ExecutePlayerMove(P(0, 0), P(3, 0));

        var act = () => game.ExecuteBobailMove(P(2, 4));

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Bobail can only move one square.");
    }

    [Fact]
    public void Valid_Player_Moves_Contain_Only_Farthest_Destinations()
    {
        var game = new Game();

        var moves = game.GetValidPlayerMoves(P(0, 0));

        moves.Should().Contain(P(3, 0));
        moves.Should().Contain(P(1, 1));
        moves.Should().NotContain(P(1, 0));
    }

    [Fact]
    public void Bobail_Reaching_Red_Home_Row_Makes_Red_Win()
    {
        var game = new Game();

        MoveBobailTo(game, P(0, 2));
        GameRules.CheckVictory(game);

        game.Status.Should().Be(GameStatus.Finished);
        game.Winner.Should().Be(PlayerColor.Red);
    }

    [Fact]
    public void Surrounded_Bobail_Makes_Current_Player_Win()
    {
        var game = new Game();
        game.Board.Pieces.Clear();
        game.Board.Pieces.Add(BobailAt(2, 2));
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

        GameRules.CheckVictory(game);

        game.Status.Should().Be(GameStatus.Finished);
        game.Winner.Should().Be(PlayerColor.Red);
    }

    private static void MoveBobailTo(Game game, Position target)
    {
        var bobail = game.Board.Pieces.Single(piece => piece.IsBobail);
        game.Board.MovePiece(bobail, target);
    }

    private static Piece BobailAt(int row, int column)
    {
        return new Piece(PieceType.Bobail, P(row, column));
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
