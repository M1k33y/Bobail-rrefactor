using Bobail.Domain.Games;
using Bobail.Infrastructure.Persistance.Entities;
using Bobail.Infrastructure.Persistance.Repositories;
using Bobail.Infrastructure.Tests.TestSupport;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Bobail.Infrastructure.Tests.Persistance.Repositories;

public class SqlGameRepositoryTests
{
    [Fact]
    public async Task AddAsync_And_GetByIdAsync_RoundTrip_Game()
    {
        using var db = InfrastructureTestDb.Create();
        var repository = new SqlGameRepository(db.Context);
        var game = new Game(GameMode.PlayerVsBot, BotDifficulty.Medium, PlayerColor.Green);

        await repository.AddAsync(game);
        var restored = await repository.GetByIdAsync(game.Id);

        restored.Should().NotBeNull();
        restored!.Id.Should().Be(game.Id);
        restored.Mode.Should().Be(GameMode.PlayerVsBot);
        restored.BotDifficulty.Should().Be(BotDifficulty.Medium);
        restored.BotColor.Should().Be(PlayerColor.Green);
        restored.Status.Should().Be(GameStatus.InProgress);
        (await repository.ExistsAsync(game.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Persists_State_And_Winner_User()
    {
        using var db = InfrastructureTestDb.Create();
        var repository = new SqlGameRepository(db.Context);
        var game = new Game();
        var winnerUserId = Guid.NewGuid();

        await repository.AddAsync(game);
        db.Context.Users.Add(UserEntity(winnerUserId));
        db.Context.GamePlayers.Add(new GamePlayerEntity
        {
            Id = Guid.NewGuid(),
            GameId = game.Id,
            UserId = winnerUserId,
            Color = (int)PlayerColor.Red,
            IsBot = false
        });
        await db.Context.SaveChangesAsync();

        game.Finish(PlayerColor.Red);
        await repository.UpdateAsync(game);

        var entity = await db.Context.Games.SingleAsync(x => x.Id == game.Id);
        entity.Status.Should().Be((int)GameStatus.Finished);
        entity.WinnerUserId.Should().Be(winnerUserId);

        var restored = await repository.GetByIdAsync(game.Id);
        restored!.Status.Should().Be(GameStatus.Finished);
        restored.Winner.Should().Be(PlayerColor.Red);
    }

    [Fact]
    public async Task DeleteAsync_Removes_Game()
    {
        using var db = InfrastructureTestDb.Create();
        var repository = new SqlGameRepository(db.Context);
        var game = new Game();

        await repository.AddAsync(game);
        await repository.DeleteAsync(game.Id);

        (await repository.GetByIdAsync(game.Id)).Should().BeNull();
        (await repository.ExistsAsync(game.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_When_Game_Is_Missing_Throws()
    {
        using var db = InfrastructureTestDb.Create();
        var repository = new SqlGameRepository(db.Context);

        var act = () => repository.UpdateAsync(new Game());

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Game not found.");
    }

    private static UserEntity UserEntity(Guid id)
    {
        return new UserEntity
        {
            Id = id,
            Email = $"{id:N}@mail.com",
            Nickname = $"user-{id:N}",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }
}
