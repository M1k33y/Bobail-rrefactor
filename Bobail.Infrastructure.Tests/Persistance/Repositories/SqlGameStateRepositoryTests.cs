using Bobail.Domain.Games;
using Bobail.Infrastructure.Persistance.Repositories;
using Bobail.Infrastructure.Tests.TestSupport;
using FluentAssertions;

namespace Bobail.Infrastructure.Tests.Persistance.Repositories;

public class SqlGameStateRepositoryTests
{
    [Fact]
    public async Task AddSnapshotAsync_Assigns_Sequential_Move_Numbers()
    {
        using var db = InfrastructureTestDb.Create();
        var gameRepository = new SqlGameRepository(db.Context);
        var stateRepository = new SqlGameStateRepository(db.Context);
        var game = new Game();

        await gameRepository.AddAsync(game);
        await stateRepository.AddSnapshotAsync(game);
        game.ExecutePlayerMove(P(0, 0), P(3, 0));
        await stateRepository.AddSnapshotAsync(game);

        var records = await stateRepository.GetByGameIdAsync(game.Id);

        records.Should().HaveCount(2);
        records.Select(x => x.MoveNumber).Should().Equal(0, 1);
        records[0].StateJson.Should().NotBe(records[1].StateJson);
    }

    [Fact]
    public async Task GetByGameIdAsync_Returns_Only_Requested_Game_Ordered_By_Move()
    {
        using var db = InfrastructureTestDb.Create();
        var gameRepository = new SqlGameRepository(db.Context);
        var stateRepository = new SqlGameStateRepository(db.Context);
        var requestedGame = new Game();
        var otherGame = new Game();

        await gameRepository.AddAsync(requestedGame);
        await gameRepository.AddAsync(otherGame);
        await stateRepository.AddSnapshotAsync(requestedGame);
        await stateRepository.AddSnapshotAsync(otherGame);
        requestedGame.ExecutePlayerMove(P(0, 0), P(3, 0));
        await stateRepository.AddSnapshotAsync(requestedGame);

        var records = await stateRepository.GetByGameIdAsync(requestedGame.Id);

        records.Should().HaveCount(2);
        records.Select(x => x.MoveNumber).Should().Equal(0, 1);
    }

    private static Position P(int row, int column)
    {
        return new Position(row, column);
    }
}
