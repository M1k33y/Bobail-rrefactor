using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bobail.Application.DTOs;
using Bobail.Domain.Games;
using Bobail.Infrastructure.Persistance;
using Bobail.Infrastructure.Persistance.Entities;
using Bobail.Infrastructure.Persistence;
using Bobail.IntegrationTests.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bobail.IntegrationTests;

public class AdminIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private const string ValidPassword = "StrongPass1";

    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AdminIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Admin_Should_List_And_Toggle_Users()
    {
        var admin = CreateUser("admin", role: 1, isActive: true);
        var target = CreateUser("target", role: 0, isActive: true);

        await SeedUsersAsync(admin, target);

        var token = await LoginAsync(admin.Email);
        using var usersRequest = CreateAuthorizedRequest(
            HttpMethod.Get,
            $"/api/admin/users?search={target.Email}",
            token);

        var usersResponse = await _client.SendAsync(usersRequest);

        Assert.Equal(HttpStatusCode.OK, usersResponse.StatusCode);

        var users = await usersResponse.Content.ReadFromJsonAsync<PagedAdminUsersResponse>();
        Assert.NotNull(users);
        Assert.Contains(users!.Items, x => x.Id == target.Id);

        using var banRequest = CreateAuthorizedRequest(
            HttpMethod.Patch,
            $"/api/admin/users/{target.Id}/ban",
            token);

        var banResponse = await _client.SendAsync(banRequest);

        Assert.Equal(HttpStatusCode.OK, banResponse.StatusCode);

        var banResult = await banResponse.Content.ReadFromJsonAsync<BanUserResponse>();
        Assert.NotNull(banResult);
        Assert.False(banResult!.User.IsActive);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        var storedTarget = await dbContext.Users.FirstAsync(x => x.Id == target.Id);
        Assert.False(storedTarget.IsActive);
    }

    [Fact]
    public async Task NonAdmin_Should_Not_Access_Admin_Endpoints()
    {
        var user = CreateUser("player", role: 0, isActive: true);

        await SeedUsersAsync(user);

        var token = await LoginAsync(user.Email);
        using var request = CreateAuthorizedRequest(HttpMethod.Get, "/api/admin/users", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task BanUser_Should_Finish_Active_Online_Game_With_Opponent_As_Winner()
    {
        var admin = CreateUser("admin", role: 1, isActive: true);
        var red = CreateUser("red", role: 0, isActive: true);
        var green = CreateUser("green", role: 0, isActive: true);
        var game = new Game(GameMode.OnlineMultiplayer);
        game.Start();

        await SeedUsersAsync(admin, red, green);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
            dbContext.Games.Add(new GameEntity
            {
                Id = game.Id,
                StateJson = GameSerializer.Serialize(game),
                Status = (int)game.Status,
                CurrentTurn = (int)game.CurrentTurn,
                Mode = (int)game.Mode,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            dbContext.GamePlayers.AddRange(
                new GamePlayerEntity
                {
                    Id = Guid.NewGuid(),
                    GameId = game.Id,
                    UserId = red.Id,
                    Color = (int)PlayerColor.Red,
                    IsBot = false
                },
                new GamePlayerEntity
                {
                    Id = Guid.NewGuid(),
                    GameId = game.Id,
                    UserId = green.Id,
                    Color = (int)PlayerColor.Green,
                    IsBot = false
                });
            await dbContext.SaveChangesAsync();
        }

        var adminToken = await LoginAsync(admin.Email);
        using var banRequest = CreateAuthorizedRequest(
            HttpMethod.Patch,
            $"/api/admin/users/{red.Id}/ban",
            adminToken);

        var banResponse = await _client.SendAsync(banRequest);

        Assert.Equal(HttpStatusCode.OK, banResponse.StatusCode);

        var banResult = await banResponse.Content.ReadFromJsonAsync<BanUserResponse>();
        Assert.NotNull(banResult);
        Assert.Single(banResult!.FinishedGames);
        Assert.Equal(PlayerColor.Green.ToString(), banResult.FinishedGames[0].Winner);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
            var storedGame = await dbContext.Games.FirstAsync(x => x.Id == game.Id);
            Assert.Equal((int)GameStatus.Finished, storedGame.Status);
            Assert.Equal(green.Id, storedGame.WinnerUserId);
        }
    }

    private static UserEntity CreateUser(string prefix, int role, bool isActive)
    {
        return new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = $"{prefix}_{Guid.NewGuid()}@mail.com",
            Nickname = prefix,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(ValidPassword),
            Role = role,
            CreatedAt = DateTime.UtcNow,
            IsActive = isActive,
            IsEmailVerified = true,
            EmailVerifiedAtUtc = DateTime.UtcNow
        };
    }

    private async Task SeedUsersAsync(params UserEntity[] users)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();

        dbContext.Users.AddRange(users);
        await dbContext.SaveChangesAsync();
    }

    private async Task<string> LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = ValidPassword,
            RememberMe = false
        });

        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResponse);

        return loginResponse!.Token;
    }

    private static HttpRequestMessage CreateAuthorizedRequest(
        HttpMethod method,
        string url,
        string token)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return request;
    }
}
