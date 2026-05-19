using Bobail.Application.DTOs;
using Bobail.Application.Interfaces.Repositories;
using Bobail.Application.Interfaces.Services;
using Bobail.Application.Services;
using Bobail.Domain.Common;
using Bobail.Domain.Games;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bobail.Application.Tests.Services;

public class OnlineGameServiceTests
{
    [Fact]
    public async Task CreateOnlineGameAsync_Persists_Game_Adds_Creator_And_Snapshot()
    {
        Game? createdGame = null;
        var gameRepository = new Mock<IGameRepository>();
        var gamePlayerRepository = new Mock<IGamePlayerRepository>();
        var gameStateRepository = new Mock<IGameStateRepository>();
        var service = CreateService(
            gameRepository,
            gameStateRepository,
            gamePlayerRepository);
        var userId = Guid.NewGuid();

        gameRepository
            .Setup(x => x.AddAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Callback<Game, CancellationToken>((game, _) => createdGame = game)
            .Returns(Task.CompletedTask);
        gamePlayerRepository
            .Setup(x => x.AddOnlinePlayerAsync(It.IsAny<Guid>(), userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PlayerColor.Red);

        var gameId = await service.CreateOnlineGameAsync(userId);

        gameId.Should().NotBeEmpty();
        createdGame.Should().NotBeNull();
        createdGame!.Mode.Should().Be(GameMode.OnlineMultiplayer);
        createdGame.Status.Should().Be(GameStatus.WaitingForPlayers);
        gamePlayerRepository.Verify(
            x => x.AddOnlinePlayerAsync(gameId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
        gameStateRepository.Verify(
            x => x.AddSnapshotAsync(createdGame, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task JoinOnlineGameAsync_When_User_Already_In_Game_Returns_Existing_Color()
    {
        var game = new Game(GameMode.OnlineMultiplayer);
        var gameRepository = new Mock<IGameRepository>();
        var gamePlayerRepository = new Mock<IGamePlayerRepository>();
        var service = CreateService(gameRepository, gamePlayerRepository: gamePlayerRepository);
        var userId = Guid.NewGuid();

        gameRepository
            .Setup(x => x.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);
        gamePlayerRepository
            .Setup(x => x.GetPlayerColorAsync(game.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PlayerColor.Red);
        gamePlayerRepository
            .Setup(x => x.CountHumanPlayersAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var response = await service.JoinOnlineGameAsync(game.Id, userId);

        response.PlayerColor.Should().Be("Red");
        response.Status.Should().Be(GameStatus.WaitingForPlayers.ToString());
        gamePlayerRepository.Verify(
            x => x.AddOnlinePlayerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task JoinOnlineGameAsync_When_Second_Player_Joins_Starts_Game()
    {
        var game = new Game(GameMode.OnlineMultiplayer);
        var gameRepository = new Mock<IGameRepository>();
        var gamePlayerRepository = new Mock<IGamePlayerRepository>();
        var service = CreateService(gameRepository, gamePlayerRepository: gamePlayerRepository);
        var userId = Guid.NewGuid();

        gameRepository
            .Setup(x => x.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);
        gamePlayerRepository
            .Setup(x => x.GetPlayerColorAsync(game.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerColor?)null);
        gamePlayerRepository
            .Setup(x => x.AddOnlinePlayerAsync(game.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PlayerColor.Green);
        gamePlayerRepository
            .Setup(x => x.CountHumanPlayersAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var response = await service.JoinOnlineGameAsync(game.Id, userId);

        game.Status.Should().Be(GameStatus.InProgress);
        response.PlayerColor.Should().Be("Green");
        response.Status.Should().Be(GameStatus.InProgress.ToString());
        gameRepository.Verify(
            x => x.UpdateAsync(game, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task JoinOnlineGameAsync_When_Game_Already_Started_Rejects_New_Player()
    {
        var game = new Game(GameMode.OnlineMultiplayer);
        game.Start();
        var gameRepository = new Mock<IGameRepository>();
        var gamePlayerRepository = new Mock<IGamePlayerRepository>();
        var service = CreateService(gameRepository, gamePlayerRepository: gamePlayerRepository);
        var userId = Guid.NewGuid();

        gameRepository
            .Setup(x => x.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);
        gamePlayerRepository
            .Setup(x => x.GetPlayerColorAsync(game.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerColor?)null);

        var act = () => service.JoinOnlineGameAsync(game.Id, userId);

        await act.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("Game is not accepting new players.");
        gamePlayerRepository.Verify(
            x => x.AddOnlinePlayerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetGameStateForUserAsync_When_User_Is_Not_Player_Throws()
    {
        var game = new Game(GameMode.OnlineMultiplayer);
        var gameRepository = new Mock<IGameRepository>();
        var gamePlayerRepository = new Mock<IGamePlayerRepository>();
        var service = CreateService(gameRepository, gamePlayerRepository: gamePlayerRepository);
        var userId = Guid.NewGuid();

        gameRepository
            .Setup(x => x.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);
        gamePlayerRepository
            .Setup(x => x.GetPlayerColorAsync(game.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerColor?)null);

        var act = () => service.GetGameStateForUserAsync(game.Id, userId);

        await act.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("User does not belong to this game.");
    }

    [Fact]
    public async Task ExecutePlayerMoveAsync_When_Not_Player_Turn_Does_Not_Persist()
    {
        var game = StartedOnlineGame();
        var gameRepository = new Mock<IGameRepository>();
        var gamePlayerRepository = new Mock<IGamePlayerRepository>();
        var gameStateRepository = new Mock<IGameStateRepository>();
        var service = CreateService(
            gameRepository,
            gameStateRepository,
            gamePlayerRepository);
        var userId = Guid.NewGuid();

        gameRepository
            .Setup(x => x.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);
        gamePlayerRepository
            .Setup(x => x.GetPlayerColorAsync(game.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PlayerColor.Green);

        var act = () => service.ExecutePlayerMoveAsync(
            game.Id,
            userId,
            new PlayerMoveRequest { FromRow = 4, FromColumn = 0, ToRow = 1, ToColumn = 0 });

        await act.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("It is not your turn.");
        gameRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()),
            Times.Never);
        gameStateRepository.Verify(
            x => x.AddSnapshotAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecutePlayerMoveAsync_With_Valid_Move_Persists_And_Returns_Result()
    {
        var game = StartedOnlineGame();
        var gameRepository = new Mock<IGameRepository>();
        var gamePlayerRepository = new Mock<IGamePlayerRepository>();
        var gameStateRepository = new Mock<IGameStateRepository>();
        var service = CreateService(
            gameRepository,
            gameStateRepository,
            gamePlayerRepository);
        var userId = Guid.NewGuid();

        gameRepository
            .Setup(x => x.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);
        gamePlayerRepository
            .Setup(x => x.GetPlayerColorAsync(game.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PlayerColor.Red);

        var result = await service.ExecutePlayerMoveAsync(
            game.Id,
            userId,
            new PlayerMoveRequest { FromRow = 0, FromColumn = 0, ToRow = 3, ToColumn = 0 });

        result.MoveType.Should().Be("PlayerMove");
        result.PlayerColor.Should().Be("Red");
        result.Game.CurrentTurn.Should().Be("Green");
        gameRepository.Verify(
            x => x.UpdateAsync(game, It.IsAny<CancellationToken>()),
            Times.Once);
        gameStateRepository.Verify(
            x => x.AddSnapshotAsync(game, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteBobailMoveAsync_With_Valid_Move_Persists_And_Returns_Result()
    {
        var game = StartedOnlineGame();
        game.ExecutePlayerMove(P(0, 0), P(3, 0));
        var gameRepository = new Mock<IGameRepository>();
        var gamePlayerRepository = new Mock<IGamePlayerRepository>();
        var gameStateRepository = new Mock<IGameStateRepository>();
        var service = CreateService(
            gameRepository,
            gameStateRepository,
            gamePlayerRepository);
        var userId = Guid.NewGuid();

        gameRepository
            .Setup(x => x.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);
        gamePlayerRepository
            .Setup(x => x.GetPlayerColorAsync(game.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PlayerColor.Green);

        var result = await service.ExecuteBobailMoveAsync(
            game.Id,
            userId,
            new BobailMoveRequest { ToRow = 2, ToColumn = 1 });

        result.MoveType.Should().Be("BobailMove");
        result.PlayerColor.Should().Be("Green");
        result.Game.CurrentPhase.Should().Be(TurnPhase.PlayerMoveRequired.ToString());
        gameRepository.Verify(
            x => x.UpdateAsync(game, It.IsAny<CancellationToken>()),
            Times.Once);
        gameStateRepository.Verify(
            x => x.AddSnapshotAsync(game, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecutePlayerMoveAsync_Disposes_Game_Lock_Even_When_Move_Is_Rejected()
    {
        var game = StartedOnlineGame();
        var gameLock = new TestGameLock();
        var lockManager = new Mock<IGameLockManager>();
        var gameRepository = new Mock<IGameRepository>();
        var gamePlayerRepository = new Mock<IGamePlayerRepository>();
        var service = CreateService(
            gameRepository,
            gamePlayerRepository: gamePlayerRepository,
            lockManager: lockManager);
        var userId = Guid.NewGuid();

        lockManager
            .Setup(x => x.AcquireAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameLock);
        gameRepository
            .Setup(x => x.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);
        gamePlayerRepository
            .Setup(x => x.GetPlayerColorAsync(game.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PlayerColor.Green);

        var act = () => service.ExecutePlayerMoveAsync(
            game.Id,
            userId,
            new PlayerMoveRequest { FromRow = 4, FromColumn = 0, ToRow = 1, ToColumn = 0 });

        await act.Should().ThrowAsync<DomainException>();
        gameLock.Disposed.Should().BeTrue();
    }

    private static OnlineGameService CreateService(
        Mock<IGameRepository>? gameRepository = null,
        Mock<IGameStateRepository>? gameStateRepository = null,
        Mock<IGamePlayerRepository>? gamePlayerRepository = null,
        Mock<IGameLockManager>? lockManager = null)
    {
        lockManager ??= new Mock<IGameLockManager>();
        lockManager
            .Setup(x => x.AcquireAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestGameLock());

        return new OnlineGameService(
            (gameRepository ?? new Mock<IGameRepository>()).Object,
            (gameStateRepository ?? new Mock<IGameStateRepository>()).Object,
            (gamePlayerRepository ?? new Mock<IGamePlayerRepository>()).Object,
            lockManager.Object,
            Mock.Of<ILogger<OnlineGameService>>());
    }

    private static Game StartedOnlineGame()
    {
        var game = new Game(GameMode.OnlineMultiplayer);
        game.Start();
        return game;
    }

    private static Position P(int row, int column)
    {
        return new Position(row, column);
    }

    private sealed class TestGameLock : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
