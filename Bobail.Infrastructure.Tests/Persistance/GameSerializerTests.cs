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
    public void Deserialize_When_Json_Is_Null_Value_Throws_Clear_Exception()
    {
        var act = () => GameSerializer.Deserialize("null");

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Failed to deserialize game.");
    }

    private static Position P(int row, int column)
    {
        return new Position(row, column);
    }
}
