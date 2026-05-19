using Bobail.Domain.Common;
using Bobail.Domain.Games;
using FluentAssertions;

namespace Bobail.Domain.Tests.Games;

public class GameLifecycleTests
{
    [Fact]
    public void Online_Game_Starts_Waiting_For_Players()
    {
        var game = new Game(GameMode.OnlineMultiplayer);

        game.Status.Should().Be(GameStatus.WaitingForPlayers);
        game.Mode.Should().Be(GameMode.OnlineMultiplayer);
        game.CurrentTurn.Should().Be(PlayerColor.Red);
    }

    [Fact]
    public void Starting_Online_Game_Marks_It_In_Progress()
    {
        var game = new Game(GameMode.OnlineMultiplayer);

        game.Start();

        game.Status.Should().Be(GameStatus.InProgress);
    }

    [Fact]
    public void Starting_Local_Game_Throws()
    {
        var game = new Game();

        var act = () => game.Start();

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Only online games can wait for players.");
    }

    [Fact]
    public void Abandoned_Game_Rejects_Player_Moves()
    {
        var game = new Game();
        game.Abandon();

        var act = () => game.ExecutePlayerMove(P(0, 0), P(3, 0));

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Game is not active.");
    }

    private static Position P(int row, int column)
    {
        return new Position(row, column);
    }
}
