using Bobail.Domain.Common;
using Bobail.Domain.Games;
using Bobail.Infrastructure.Persistance.Entities;
using Bobail.Infrastructure.Persistance.Repositories;
using Bobail.Infrastructure.Tests.TestSupport;
using FluentAssertions;

namespace Bobail.Infrastructure.Tests.Persistance.Repositories;

public class SqlGamePlayerRepositoryTests
{
    [Fact]
    public async Task AddPlayersForGame_For_Bot_Game_Adds_Human_And_Bot_Slots()
    {
        using var db = InfrastructureTestDb.Create();
        var repository = new SqlGamePlayerRepository(db.Context);
        var game = await AddGameAsync(db);
        var userId = await AddUserAsync(db);

        await repository.AddPlayersForGame(game.Id, userId, isVsBot: true, botColor: PlayerColor.Red);

        var players = db.Context.GamePlayers
            .Where(x => x.GameId == game.Id)
            .ToList();
        players.Should().HaveCount(2);
        players.Should().Contain(x =>
            x.UserId == userId &&
            x.Color == (int)PlayerColor.Green &&
            !x.IsBot);
        players.Should().Contain(x =>
            x.UserId == null &&
            x.Color == (int)PlayerColor.Red &&
            x.IsBot);
    }

    [Fact]
    public async Task AddPlayersForGame_When_User_Already_Participates_Does_Not_Duplicate()
    {
        using var db = InfrastructureTestDb.Create();
        var repository = new SqlGamePlayerRepository(db.Context);
        var game = await AddGameAsync(db);
        var userId = await AddUserAsync(db);

        await repository.AddPlayersForGame(game.Id, userId, isVsBot: false);
        await repository.AddPlayersForGame(game.Id, userId, isVsBot: false);

        db.Context.GamePlayers
            .Where(x => x.GameId == game.Id && x.UserId == userId)
            .Should()
            .ContainSingle();
    }

    [Fact]
    public async Task AddOnlinePlayerAsync_Assigns_Red_Then_Green_And_Returns_Existing_Color()
    {
        using var db = InfrastructureTestDb.Create();
        var repository = new SqlGamePlayerRepository(db.Context);
        var game = await AddGameAsync(db);
        var firstUserId = await AddUserAsync(db);
        var secondUserId = await AddUserAsync(db);

        var firstColor = await repository.AddOnlinePlayerAsync(game.Id, firstUserId);
        var repeatedColor = await repository.AddOnlinePlayerAsync(game.Id, firstUserId);
        var secondColor = await repository.AddOnlinePlayerAsync(game.Id, secondUserId);

        firstColor.Should().Be(PlayerColor.Red);
        repeatedColor.Should().Be(PlayerColor.Red);
        secondColor.Should().Be(PlayerColor.Green);
        (await repository.CountHumanPlayersAsync(game.Id)).Should().Be(2);
        (await repository.GetPlayerColorAsync(game.Id, secondUserId)).Should().Be(PlayerColor.Green);
        (await repository.UserParticipatesInGameAsync(game.Id, firstUserId)).Should().BeTrue();
    }

    [Fact]
    public async Task AddOnlinePlayerAsync_When_Game_Has_Two_Humans_Throws()
    {
        using var db = InfrastructureTestDb.Create();
        var repository = new SqlGamePlayerRepository(db.Context);
        var game = await AddGameAsync(db);
        await repository.AddOnlinePlayerAsync(game.Id, await AddUserAsync(db));
        await repository.AddOnlinePlayerAsync(game.Id, await AddUserAsync(db));
        var thirdUserId = await AddUserAsync(db);

        var act = () => repository.AddOnlinePlayerAsync(game.Id, thirdUserId);

        await act.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("Game already has two players.");
    }

    private static async Task<Game> AddGameAsync(InfrastructureTestDb db)
    {
        var game = new Game();
        await new SqlGameRepository(db.Context).AddAsync(game);
        return game;
    }

    private static async Task<Guid> AddUserAsync(InfrastructureTestDb db)
    {
        var userId = Guid.NewGuid();
        db.Context.Users.Add(new UserEntity
        {
            Id = userId,
            Email = $"{userId:N}@mail.com",
            Nickname = $"user-{userId:N}",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await db.Context.SaveChangesAsync();
        return userId;
    }
}
