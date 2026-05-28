using Bobail.Domain.Games;
using Bobail.Infrastructure.Persistance;
using Bobail.Infrastructure.Persistance.Entities;
using Bobail.Infrastructure.Persistance.Repositories;
using Bobail.Infrastructure.Tests.TestSupport;
using FluentAssertions;

namespace Bobail.Infrastructure.Tests.Persistance.Repositories;

public class SqlGameHistoryRepositoryTests
{
    [Fact]
    public async Task GetHistoryForUserAsync_When_User_Has_No_Games_Returns_Empty_Page()
    {
        using var db = InfrastructureTestDb.Create();
        var repository = CreateRepository(db);

        var result = await repository.GetHistoryForUserAsync(
            Guid.NewGuid(),
            new() { Page = 2, PageSize = 10 });

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetHistoryForUserAsync_Returns_Finished_Game_With_Bot_Label()
    {
        using var db = InfrastructureTestDb.Create();
        var userId = await AddUserAsync(db, "player");
        var gameId = await AddFinishedBotGameAsync(
            db,
            userId,
            PlayerColor.Red,
            winnerUserId: userId);
        var repository = CreateRepository(db);

        var result = await repository.GetHistoryForUserAsync(
            userId,
            new() { Page = 1, PageSize = 10 });

        result.TotalCount.Should().Be(1);
        result.TotalPages.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].GameId.Should().Be(gameId);
        result.Items[0].OpponentName.Should().Be("BOT");
        result.Items[0].PlayedVs.Should().Be("BOT Hard");
        result.Items[0].Result.Should().Be("Win");
        result.Items[0].EndReason.Should().Be(GameEndReason.Victory.ToString());
        result.Items[0].BotDifficulty.Should().Be("Hard");
    }

    [Fact]
    public async Task GetUserStatsAsync_Counts_Wins_Losses_And_Color_Splits()
    {
        using var db = InfrastructureTestDb.Create();
        var userId = await AddUserAsync(db, "player");
        await AddFinishedBotGameAsync(db, userId, PlayerColor.Red, winnerUserId: userId);
        await AddFinishedBotGameAsync(db, userId, PlayerColor.Green, winnerUserId: null);
        var repository = CreateRepository(db);

        var stats = await repository.GetUserStatsAsync(userId);

        stats.Should().NotBeNull();
        stats!.TotalGamesPlayed.Should().Be(2);
        stats.TotalWins.Should().Be(1);
        stats.TotalLosses.Should().Be(1);
        stats.WinsWithRed.Should().Be(1);
        stats.LossesWithGreen.Should().Be(1);
    }

    [Fact]
    public async Task GetReplayAsync_When_User_Is_Not_Participant_Returns_Null()
    {
        using var db = InfrastructureTestDb.Create();
        var userId = await AddUserAsync(db, "player");
        var gameId = await AddFinishedBotGameAsync(db, userId, PlayerColor.Red, winnerUserId: userId);
        var repository = CreateRepository(db);

        var replay = await repository.GetReplayAsync(gameId, Guid.NewGuid());

        replay.Should().BeNull();
    }

    [Fact]
    public async Task GetReplayAsync_Returns_States_For_Finished_Game()
    {
        using var db = InfrastructureTestDb.Create();
        var userId = await AddUserAsync(db, "player");
        var gameId = await AddFinishedBotGameAsync(db, userId, PlayerColor.Red, winnerUserId: userId);
        var repository = CreateRepository(db);

        var replay = await repository.GetReplayAsync(gameId, userId);

        replay.Should().NotBeNull();
        replay!.GameId.Should().Be(gameId);
        replay.Result.Should().Be("Win");
        replay.PlayedVs.Should().Be("BOT Hard");
        replay.States.Should().ContainSingle();
        replay.States[0].Status.Should().Be(GameStatus.Finished.ToString());
        replay.States[0].Winner.Should().Be(PlayerColor.Red.ToString());
        replay.States[0].EndReason.Should().Be(GameEndReason.Victory.ToString());
        replay.States[0].Pieces.Should().HaveCount(11);
    }

    [Fact]
    public async Task GetReplayAsync_Maps_Online_Clock_For_Replay_States()
    {
        using var db = InfrastructureTestDb.Create();
        var redUserId = await AddUserAsync(db, "red");
        var greenUserId = await AddUserAsync(db, "green");
        var startedAtUtc = DateTimeOffset.Parse("2026-05-28T12:00:00Z");
        var game = new Game(GameMode.OnlineMultiplayer);
        game.Start();
        game.StartClock(TimeControl.Create(TimeSpan.FromMinutes(3)), startedAtUtc);
        game.Clock!.CommitElapsed(PlayerColor.Red, startedAtUtc.AddSeconds(2));
        game.Finish(PlayerColor.Green, GameEndReason.Timeout);

        db.Context.Games.Add(new GameEntity
        {
            Id = game.Id,
            StateJson = GameSerializer.Serialize(game),
            Status = (int)GameStatus.Finished,
            CurrentTurn = (int)game.CurrentTurn,
            Mode = (int)GameMode.OnlineMultiplayer,
            WinnerUserId = greenUserId,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            UpdatedAt = DateTime.UtcNow
        });

        db.Context.GamePlayers.AddRange(
            new GamePlayerEntity
            {
                Id = Guid.NewGuid(),
                GameId = game.Id,
                UserId = redUserId,
                Color = (int)PlayerColor.Red,
                IsBot = false
            },
            new GamePlayerEntity
            {
                Id = Guid.NewGuid(),
                GameId = game.Id,
                UserId = greenUserId,
                Color = (int)PlayerColor.Green,
                IsBot = false
            });

        await db.Context.SaveChangesAsync();
        await new SqlGameStateRepository(db.Context).AddSnapshotAsync(game);
        var repository = CreateRepository(db);

        var replay = await repository.GetReplayAsync(game.Id, redUserId);

        replay.Should().NotBeNull();
        replay!.States.Should().ContainSingle();
        replay.States[0].Clock.Should().NotBeNull();
        var clock = replay.States[0].Clock!;
        clock.RedRemainingMilliseconds.Should().Be(178_000);
        clock.GreenRemainingMilliseconds.Should().Be(180_000);
        replay.EndReason.Should().Be(GameEndReason.Timeout.ToString());
        replay.States[0].EndReason.Should().Be(GameEndReason.Timeout.ToString());
    }

    private static SqlGameHistoryRepository CreateRepository(InfrastructureTestDb db)
    {
        return new SqlGameHistoryRepository(
            db.Context,
            new SqlGameStateRepository(db.Context));
    }

    private static async Task<Guid> AddFinishedBotGameAsync(
        InfrastructureTestDb db,
        Guid userId,
        PlayerColor userColor,
        Guid? winnerUserId)
    {
        var game = new Game(GameMode.PlayerVsBot, BotDifficulty.Hard, Opponent(userColor));
        game.Finish(winnerUserId == userId ? userColor : Opponent(userColor));

        db.Context.Games.Add(new GameEntity
        {
            Id = game.Id,
            StateJson = GameSerializer.Serialize(game),
            Status = (int)GameStatus.Finished,
            CurrentTurn = (int)game.CurrentTurn,
            Mode = (int)GameMode.PlayerVsBot,
            BotDifficulty = (int)BotDifficulty.Hard,
            WinnerUserId = winnerUserId,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        });

        db.Context.GamePlayers.Add(new GamePlayerEntity
        {
            Id = Guid.NewGuid(),
            GameId = game.Id,
            UserId = userId,
            Color = (int)userColor,
            IsBot = false
        });

        db.Context.GamePlayers.Add(new GamePlayerEntity
        {
            Id = Guid.NewGuid(),
            GameId = game.Id,
            UserId = null,
            Color = (int)Opponent(userColor),
            IsBot = true
        });

        await db.Context.SaveChangesAsync();
        await new SqlGameStateRepository(db.Context).AddSnapshotAsync(game);

        return game.Id;
    }

    private static async Task<Guid> AddUserAsync(InfrastructureTestDb db, string nickname)
    {
        var userId = Guid.NewGuid();
        db.Context.Users.Add(new UserEntity
        {
            Id = userId,
            Email = $"{nickname}-{userId:N}@mail.com",
            Nickname = nickname,
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow.AddMonths(-1),
            IsActive = true
        });
        await db.Context.SaveChangesAsync();
        return userId;
    }

    private static PlayerColor Opponent(PlayerColor color)
    {
        return color == PlayerColor.Red
            ? PlayerColor.Green
            : PlayerColor.Red;
    }
}
