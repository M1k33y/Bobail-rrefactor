using Bobail.Domain.Games;
using Bobail.Infrastructure.Persistance;
using FluentAssertions;

namespace Bobail.Infrastructure.Tests.Persistance;

public class GameSerializerTests
{
    [Fact]
    public void Serialize_And_Deserialize_Preserves_Game_State()
    {
        var game = new Game(GameMode.PlayerVsBot, BotDifficulty.Hard, PlayerColor.Green);
        game.ExecutePlayerMove(P(0, 0), P(3, 0));

        var json = GameSerializer.Serialize(game);
        var restored = GameSerializer.Deserialize(json);

        restored.Id.Should().Be(game.Id);
        restored.Mode.Should().Be(GameMode.PlayerVsBot);
        restored.BotDifficulty.Should().Be(BotDifficulty.Hard);
        restored.BotColor.Should().Be(PlayerColor.Green);
        restored.CurrentTurn.Should().Be(PlayerColor.Green);
        restored.CurrentPhase.Should().Be(TurnPhase.BobailMoveRequired);
        restored.Board.Pieces.Should().HaveCount(11);
    }

    [Fact]
    public void Serialize_And_Deserialize_Preserves_End_Reason()
    {
        var game = new Game();
        game.Finish(PlayerColor.Green, GameEndReason.Timeout);

        var json = GameSerializer.Serialize(game);
        var restored = GameSerializer.Deserialize(json);

        restored.Status.Should().Be(GameStatus.Finished);
        restored.Winner.Should().Be(PlayerColor.Green);
        restored.EndReason.Should().Be(GameEndReason.Timeout);
    }

    [Fact]
    public void Deserialize_When_Json_Is_Null_Value_Throws_Clear_Exception()
    {
        var act = () => GameSerializer.Deserialize("null");

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Failed to deserialize game.");
    }

    [Fact]
    public void Serialize_And_Deserialize_Preserves_Online_Game_Clock()
    {
        var startedAtUtc = DateTimeOffset.Parse("2026-05-28T12:00:00Z");
        var game = new Game(GameMode.OnlineMultiplayer);
        game.Start();
        game.StartClock(TimeControl.Create(TimeSpan.FromMinutes(3)), startedAtUtc);
        game.Clock!.CommitElapsed(PlayerColor.Red, startedAtUtc.AddSeconds(2));

        var json = GameSerializer.Serialize(game);
        var restored = GameSerializer.Deserialize(json);

        restored.Clock.Should().NotBeNull();
        restored.Clock!.TimeControl.InitialTimeMilliseconds.Should().Be(180_000);
        restored.Clock.RedRemainingMilliseconds.Should().Be(178_000);
        restored.Clock.GreenRemainingMilliseconds.Should().Be(180_000);
        restored.Clock.TurnStartedAtUtc.Should().Be(startedAtUtc.AddSeconds(2));
    }

    private static Position P(int row, int column)
    {
        return new Position(row, column);
    }
}
