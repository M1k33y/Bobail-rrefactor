using Bobail.Application.Interfaces.Services;
using Bobail.Application.Services;
using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bobail.Application.Tests.Services.Bot;

public class BotServiceTests
{
    [Fact]
    public async Task ExecuteSingleMoveAsync_When_Difficulty_Is_Not_Set_Throws()
    {
        var service = new BotService(
            Array.Empty<IBotStrategy>(),
            Mock.Of<ILogger<BotService>>());

        var act = () => service.ExecuteSingleMoveAsync(new Game());

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Bot difficulty not set.");
    }

    [Fact]
    public async Task ExecuteSingleMoveAsync_Uses_Strategy_For_Configured_Difficulty()
    {
        var game = new Game(
            GameMode.PlayerVsBot,
            BotDifficulty.Easy,
            PlayerColor.Red);
        var strategy = new StubBotStrategy(
            BotDifficulty.Easy,
            _ => BotMove.Piece(P(0, 0), P(3, 0)));
        var service = CreateService(strategy);

        await service.ExecuteSingleMoveAsync(game);

        game.CurrentTurn.Should().Be(PlayerColor.Green);
        game.CurrentPhase.Should().Be(TurnPhase.BobailMoveRequired);
        strategy.DecideMoveCalls.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteSingleMoveAsync_Applies_Bobail_Move_From_Strategy()
    {
        var game = new Game(
            GameMode.PlayerVsBot,
            BotDifficulty.Medium,
            PlayerColor.Green);
        game.ExecutePlayerMove(P(0, 0), P(3, 0));
        var strategy = new StubBotStrategy(
            BotDifficulty.Medium,
            _ => BotMove.Bobail(P(2, 1)));
        var service = CreateService(strategy);

        await service.ExecuteSingleMoveAsync(game);

        game.Board.Pieces.Single(piece => piece.IsBobail).Position.Should().Be(P(2, 1));
        game.CurrentPhase.Should().Be(TurnPhase.PlayerMoveRequired);
    }

    [Fact]
    public async Task ExecuteSingleMoveAsync_When_No_Strategy_Matches_Throws()
    {
        var game = new Game(
            GameMode.PlayerVsBot,
            BotDifficulty.Hard,
            PlayerColor.Red);
        var strategy = new StubBotStrategy(
            BotDifficulty.Easy,
            _ => BotMove.Piece(P(0, 0), P(3, 0)));
        var service = CreateService(strategy);

        var act = () => service.ExecuteSingleMoveAsync(game);

        await act.Should().ThrowAsync<InvalidOperationException>();
        strategy.DecideMoveCalls.Should().Be(0);
    }

    private static BotService CreateService(params IBotStrategy[] strategies)
    {
        return new BotService(strategies, Mock.Of<ILogger<BotService>>());
    }

    private static Position P(int row, int column)
    {
        return new Position(row, column);
    }

    private sealed class StubBotStrategy : IBotStrategy
    {
        private readonly Func<Game, BotMove> _decideMove;

        public StubBotStrategy(BotDifficulty difficulty, Func<Game, BotMove> decideMove)
        {
            Difficulty = difficulty;
            _decideMove = decideMove;
        }

        public BotDifficulty Difficulty { get; }

        public int DecideMoveCalls { get; private set; }

        public BotMove DecideMove(Game game)
        {
            DecideMoveCalls++;
            return _decideMove(game);
        }
    }
}
