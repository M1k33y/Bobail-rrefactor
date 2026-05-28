using Bobail.Application.DTOs;
using Bobail.Domain.Games;
using FluentAssertions;

namespace Bobail.Application.Tests.DTOs;

public class GameResponseMapperTests
{
    [Fact]
    public void ToResponse_Maps_Game_State_And_Player_Color()
    {
        var game = new Game(
            GameMode.PlayerVsBot,
            BotDifficulty.Hard,
            PlayerColor.Green);
        game.ExecutePlayerMove(P(0, 0), P(3, 0));

        var response = GameResponseMapper.ToResponse(game, PlayerColor.Red);

        response.Id.Should().Be(game.Id);
        response.Status.Should().Be(GameStatus.InProgress.ToString());
        response.CurrentTurn.Should().Be(PlayerColor.Green.ToString());
        response.CurrentPhase.Should().Be(TurnPhase.BobailMoveRequired.ToString());
        response.Mode.Should().Be(GameMode.PlayerVsBot.ToString());
        response.BotColor.Should().Be(PlayerColor.Green.ToString());
        response.PlayerColor.Should().Be(PlayerColor.Red.ToString());
        response.Pieces.Should().HaveCount(11);
    }

    [Fact]
    public void ToResponse_Maps_Winner_And_Piece_Details()
    {
        var game = new Game();
        game.Finish(PlayerColor.Red);

        var response = GameResponseMapper.ToResponse(game);

        response.Winner.Should().Be(PlayerColor.Red.ToString());
        response.EndReason.Should().Be(GameEndReason.Victory.ToString());
        response.PlayerColor.Should().BeNull();
        response.Pieces.Should().Contain(piece =>
            piece.Type == "Bobail" &&
            piece.Owner == null &&
            piece.Row == 2 &&
            piece.Column == 2);
        response.Pieces.Should().Contain(piece =>
            piece.Type == "PlayerPiece" &&
            piece.Owner == "Red" &&
            piece.Row == 0);
    }

    private static Position P(int row, int column)
    {
        return new Position(row, column);
    }
}
