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

public class GameServiceQueryTests
{
    [Fact]
    public async Task GetValidPlayerMovesAsync_Returns_Valid_Move_Coordinates()
    {
        var game = new Game();
        var repository = new Mock<IGameRepository>();
        var service = CreateService(repository);

        repository
            .Setup(x => x.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var moves = await service.GetValidPlayerMovesAsync(game.Id, 0, 0);

        moves.Should().Contain((3, 0));
        moves.Should().Contain((1, 1));
        moves.Should().NotContain((1, 0));
    }

    [Fact]
    public async Task GetHistoryForUserAsync_Delegates_To_History_Repository()
    {
        var userId = Guid.NewGuid();
        var query = new GameHistoryQuery { Page = 2, PageSize = 5 };
        var expected = new PagedGameHistoryResponse
        {
            Page = 2,
            PageSize = 5,
            TotalCount = 0
        };
        var historyRepository = new Mock<IGameHistoryRepository>();
        var service = CreateService(historyRepository: historyRepository);

        historyRepository
            .Setup(x => x.GetHistoryForUserAsync(userId, query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await service.GetHistoryForUserAsync(userId, query);

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetReplayAsync_When_Replay_Is_Missing_Throws()
    {
        var historyRepository = new Mock<IGameHistoryRepository>();
        var service = CreateService(historyRepository: historyRepository);

        historyRepository
            .Setup(x => x.GetReplayAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameReplayResponse?)null);

        var act = () => service.GetReplayAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("Replay not found.");
    }

    [Fact]
    public async Task GetReplayAsync_Returns_Replay_From_History_Repository()
    {
        var gameId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expected = new GameReplayResponse { GameId = gameId };
        var historyRepository = new Mock<IGameHistoryRepository>();
        var service = CreateService(historyRepository: historyRepository);

        historyRepository
            .Setup(x => x.GetReplayAsync(gameId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await service.GetReplayAsync(gameId, userId);

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task AbandonGameAsync_When_Game_Is_Missing_Throws()
    {
        var repository = new Mock<IGameRepository>();
        var service = CreateService(repository);

        repository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Game?)null);

        var act = () => service.AbandonGameAsync(Guid.NewGuid());

        await act.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("Game not found.");
    }

    private static GameService CreateService(
        Mock<IGameRepository>? repository = null,
        Mock<IGameHistoryRepository>? historyRepository = null)
    {
        return new GameService(
            (repository ?? new Mock<IGameRepository>()).Object,
            Mock.Of<IGameStateRepository>(),
            (historyRepository ?? new Mock<IGameHistoryRepository>()).Object,
            Mock.Of<IBotService>(),
            Mock.Of<ILogger<GameService>>(),
            Mock.Of<IServiceScopeFactory>());
    }
}
