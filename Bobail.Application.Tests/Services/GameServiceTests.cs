using Bobail.Application.DTOs;
using Bobail.Application.Interfaces.Repositories;
using Bobail.Application.Interfaces.Services;
using Bobail.Application.Services;
using Bobail.Domain.Common;
using Bobail.Domain.Games;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bobail.Application.Tests.Services;

public class GameServiceTests
{
    [Fact]
    public async Task CreateGameAsync_Persists_Game_And_Initial_Snapshot()
    {
        Game? createdGame = null;
        var repository = new Mock<IGameRepository>();
        var stateRepository = new Mock<IGameStateRepository>();
        var service = CreateService(repository, stateRepository);

        repository
            .Setup(x => x.AddAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Callback<Game, CancellationToken>((game, _) => createdGame = game)
            .Returns(Task.CompletedTask);

        var gameId = await service.CreateGameAsync(
            GameMode.PlayerVsBot,
            BotDifficulty.Medium,
            PlayerColor.Green);

        gameId.Should().NotBeEmpty();
        createdGame.Should().NotBeNull();
        createdGame!.Mode.Should().Be(GameMode.PlayerVsBot);
        createdGame.BotDifficulty.Should().Be(BotDifficulty.Medium);
        createdGame.BotColor.Should().Be(PlayerColor.Green);
        stateRepository.Verify(
            x => x.AddSnapshotAsync(createdGame, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetGameAsync_When_Game_Does_Not_Exist_Throws()
    {
        var repository = new Mock<IGameRepository>();
        var service = CreateService(repository);

        repository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Game?)null);

        var act = () => service.GetGameAsync(Guid.NewGuid());

        await act.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("Game not found.");
    }

    [Fact]
    public async Task ExecutePlayerMoveAsync_With_Valid_Move_Updates_Game_And_Adds_Snapshot()
    {
        var game = new Game();
        var repository = new Mock<IGameRepository>();
        var stateRepository = new Mock<IGameStateRepository>();
        var service = CreateService(repository, stateRepository);

        repository
            .Setup(x => x.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        await service.ExecutePlayerMoveAsync(game.Id, 0, 0, 3, 0);

        game.CurrentTurn.Should().Be(PlayerColor.Green);
        game.CurrentPhase.Should().Be(TurnPhase.BobailMoveRequired);
        repository.Verify(
            x => x.UpdateAsync(game, It.IsAny<CancellationToken>()),
            Times.Once);
        stateRepository.Verify(
            x => x.AddSnapshotAsync(game, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecutePlayerMoveAsync_When_Move_Invalid_Does_Not_Persist()
    {
        var game = new Game();
        var repository = new Mock<IGameRepository>();
        var stateRepository = new Mock<IGameStateRepository>();
        var service = CreateService(repository, stateRepository);

        repository
            .Setup(x => x.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var act = () => service.ExecutePlayerMoveAsync(game.Id, 4, 0, 1, 0);

        await act.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("Cannot move opponent's piece.");
        repository.Verify(
            x => x.UpdateAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()),
            Times.Never);
        stateRepository.Verify(
            x => x.AddSnapshotAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteBobailMoveAsync_With_Valid_Move_Updates_Game_And_Adds_Snapshot()
    {
        var game = new Game();
        game.ExecutePlayerMove(P(0, 0), P(3, 0));
        var repository = new Mock<IGameRepository>();
        var stateRepository = new Mock<IGameStateRepository>();
        var service = CreateService(repository, stateRepository);

        repository
            .Setup(x => x.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        await service.ExecuteBobailMoveAsync(game.Id, 2, 1);

        game.CurrentPhase.Should().Be(TurnPhase.PlayerMoveRequired);
        game.Board.Pieces.Single(piece => piece.IsBobail).Position.Should().Be(P(2, 1));
        repository.Verify(
            x => x.UpdateAsync(game, It.IsAny<CancellationToken>()),
            Times.Once);
        stateRepository.Verify(
            x => x.AddSnapshotAsync(game, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AbandonGameAsync_Marks_Game_Abandoned_And_Updates()
    {
        var game = new Game();
        var repository = new Mock<IGameRepository>();
        var service = CreateService(repository);

        repository
            .Setup(x => x.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        await service.AbandonGameAsync(game.Id);

        game.Status.Should().Be(GameStatus.Abandoned);
        repository.Verify(
            x => x.UpdateAsync(game, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteBotCycleAsync_When_Game_Is_Not_Bot_Turn_Does_Nothing()
    {
        var game = new Game();
        var repository = new Mock<IGameRepository>();
        var botService = new Mock<IBotService>();
        var stateRepository = new Mock<IGameStateRepository>();
        var service = CreateService(repository, stateRepository, botService: botService);

        repository
            .Setup(x => x.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        await service.ExecuteBotCycleAsync(game.Id, CancellationToken.None);

        botService.Verify(
            x => x.ExecuteSingleMoveAsync(It.IsAny<Game>()),
            Times.Never);
        repository.Verify(
            x => x.UpdateAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()),
            Times.Never);
        stateRepository.Verify(
            x => x.AddSnapshotAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteBotCycleAsync_When_Bot_Has_Bobail_And_Player_Phases_Executes_Both()
    {
        var game = new Game(GameMode.PlayerVsBot, BotDifficulty.Hard, PlayerColor.Green);
        game.ExecutePlayerMove(P(0, 0), P(3, 0));
        var repository = new Mock<IGameRepository>();
        var botService = new Mock<IBotService>();
        var stateRepository = new Mock<IGameStateRepository>();
        var service = CreateService(repository, stateRepository, botService: botService);

        repository
            .Setup(x => x.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);
        botService
            .Setup(x => x.ExecuteSingleMoveAsync(It.IsAny<Game>()))
            .Callback<Game>(ExecuteSimpleBotMove)
            .Returns(Task.CompletedTask);

        await service.ExecuteBotCycleAsync(game.Id, CancellationToken.None);

        botService.Verify(
            x => x.ExecuteSingleMoveAsync(game),
            Times.Exactly(2));
        repository.Verify(
            x => x.UpdateAsync(game, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        stateRepository.Verify(
            x => x.AddSnapshotAsync(game, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        game.CurrentTurn.Should().Be(PlayerColor.Red);
    }

    [Fact]
    public async Task GetUserStatsAsync_When_Stats_Are_Missing_Throws()
    {
        var historyRepository = new Mock<IGameHistoryRepository>();
        var service = CreateService(historyRepository: historyRepository);

        historyRepository
            .Setup(x => x.GetUserStatsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserGameStatsResponse?)null);

        var act = () => service.GetUserStatsAsync(Guid.NewGuid());

        await act.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("User statistics not found.");
    }

    private static GameService CreateService(
        Mock<IGameRepository>? repository = null,
        Mock<IGameStateRepository>? stateRepository = null,
        Mock<IGameHistoryRepository>? historyRepository = null,
        Mock<IBotService>? botService = null)
    {
        return new GameService(
            (repository ?? new Mock<IGameRepository>()).Object,
            (stateRepository ?? new Mock<IGameStateRepository>()).Object,
            (historyRepository ?? new Mock<IGameHistoryRepository>()).Object,
            (botService ?? new Mock<IBotService>()).Object,
            Mock.Of<ILogger<GameService>>(),
            Mock.Of<IServiceScopeFactory>());
    }

    private static void ExecuteSimpleBotMove(Game game)
    {
        if (game.CurrentPhase == TurnPhase.BobailMoveRequired)
        {
            game.ExecuteBobailMove(P(2, 1));
            return;
        }

        game.ExecutePlayerMove(P(4, 4), P(1, 4));
    }

    private static Position P(int row, int column)
    {
        return new Position(row, column);
    }
}
