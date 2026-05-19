using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;
using Bobail.Infrastructure.Bots;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bobail.Application.Tests.Services.Bot;

public class SearchBotStrategyTests
{
    [Fact]
    public void MediumBotStrategy_During_Player_Phase_Returns_Valid_Player_Move()
    {
        var game = new Game(GameMode.PlayerVsBot, BotDifficulty.Medium, PlayerColor.Red);
        var strategy = CreateMediumStrategy();

        var move = strategy.DecideMove(game);

        move.IsBobailMove.Should().BeFalse();
        game.GetValidPlayerMoves(move.From).Should().Contain(move.To);
    }

    [Fact]
    public void MediumBotStrategy_During_Bobail_Phase_Returns_Valid_Bobail_Move()
    {
        var game = new Game(GameMode.PlayerVsBot, BotDifficulty.Medium, PlayerColor.Green);
        game.ExecutePlayerMove(P(0, 0), P(3, 0));
        var strategy = CreateMediumStrategy();

        var move = strategy.DecideMove(game);

        move.IsBobailMove.Should().BeTrue();
        game.GetValidBobailMoves().Should().Contain(move.To);
    }

    [Fact]
    public void HardBotStrategy_During_Player_Phase_Returns_Valid_Player_Move()
    {
        var game = new Game(GameMode.PlayerVsBot, BotDifficulty.Hard, PlayerColor.Red);
        var strategy = CreateHardStrategy();

        var move = strategy.DecideMove(game);

        move.IsBobailMove.Should().BeFalse();
        game.GetValidPlayerMoves(move.From).Should().Contain(move.To);
    }

    [Fact]
    public void HardBotStrategy_During_Bobail_Phase_Returns_Valid_Bobail_Move()
    {
        var game = new Game(GameMode.PlayerVsBot, BotDifficulty.Hard, PlayerColor.Green);
        game.ExecutePlayerMove(P(0, 0), P(3, 0));
        var strategy = CreateHardStrategy();

        var move = strategy.DecideMove(game);

        move.IsBobailMove.Should().BeTrue();
        game.GetValidBobailMoves().Should().Contain(move.To);
    }

    private static MediumBotStrategy CreateMediumStrategy()
    {
        return new MediumBotStrategy(
            new MediumBoardEvaluator(new EvaluationWeights()),
            Mock.Of<ILogger<MediumBotStrategy>>());
    }

    private static HardBotStrategy CreateHardStrategy()
    {
        return new HardBotStrategy(
            new HardBoardEvaluator(new EvaluationWeights()),
            Mock.Of<ILogger<HardBotStrategy>>());
    }

    private static Position P(int row, int column)
    {
        return new Position(row, column);
    }
}
