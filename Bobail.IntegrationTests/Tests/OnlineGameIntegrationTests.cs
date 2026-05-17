using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bobail.Application.DTOs;
using Bobail.Domain.Games;
using Bobail.Infrastructure.Persistance.Entities;
using Bobail.Infrastructure.Persistence;
using Bobail.IntegrationTests.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bobail.IntegrationTests;

public class OnlineGameIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private const string ValidPassword = "StrongPass1";

    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public OnlineGameIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task OnlineGame_Should_Start_When_Second_Player_Joins()
    {
        var redUser = CreateUser("red");
        var greenUser = CreateUser("green");
        await SeedUsersAsync(redUser, greenUser);

        var redToken = await LoginAsync(redUser.Email);
        var greenToken = await LoginAsync(greenUser.Email);

        var createResponse = await SendAuthorizedJsonAsync(
            HttpMethod.Post,
            "/api/games/online",
            redToken);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<CreateGameResponse>();
        Assert.NotNull(created);

        var waitingState = await GetGameAsync(created!.GameId, redToken);
        Assert.Equal(GameStatus.WaitingForPlayers.ToString(), waitingState.Status);
        Assert.Equal(PlayerColor.Red.ToString(), waitingState.PlayerColor);

        var joinResponse = await SendAuthorizedJsonAsync(
            HttpMethod.Post,
            $"/api/games/{created.GameId}/join-online",
            greenToken);
        joinResponse.EnsureSuccessStatusCode();

        var joinedState = await joinResponse.Content.ReadFromJsonAsync<GameResponse>();
        Assert.NotNull(joinedState);
        Assert.Equal(GameStatus.InProgress.ToString(), joinedState!.Status);
        Assert.Equal(PlayerColor.Green.ToString(), joinedState.PlayerColor);

        var redState = await GetGameAsync(created.GameId, redToken);
        Assert.Equal(PlayerColor.Red.ToString(), redState.PlayerColor);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        var playerCount = await dbContext.GamePlayers.CountAsync(x => x.GameId == created.GameId && !x.IsBot);
        var storedGame = await dbContext.Games.FirstAsync(x => x.Id == created.GameId);

        Assert.Equal(2, playerCount);
        Assert.Equal((int)GameStatus.InProgress, storedGame.Status);
    }

    [Fact]
    public async Task OnlineGame_Should_Reject_Out_Of_Turn_Move()
    {
        var redUser = CreateUser("red");
        var greenUser = CreateUser("green");
        await SeedUsersAsync(redUser, greenUser);

        var redToken = await LoginAsync(redUser.Email);
        var greenToken = await LoginAsync(greenUser.Email);
        var gameId = await CreateAndJoinOnlineGameAsync(redToken, greenToken);

        var response = await SendAuthorizedJsonAsync(
            HttpMethod.Post,
            $"/api/games/{gameId}/player-move",
            greenToken,
            new PlayerMoveRequest
            {
                FromRow = 4,
                FromColumn = 0,
                ToRow = 1,
                ToColumn = 0
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var state = await GetGameAsync(gameId, redToken);
        Assert.Equal(PlayerColor.Red.ToString(), state.CurrentTurn);
        Assert.Equal(TurnPhase.PlayerMoveRequired.ToString(), state.CurrentPhase);
    }

    private async Task<Guid> CreateAndJoinOnlineGameAsync(string redToken, string greenToken)
    {
        var createResponse = await SendAuthorizedJsonAsync(
            HttpMethod.Post,
            "/api/games/online",
            redToken);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<CreateGameResponse>();
        Assert.NotNull(created);

        var joinResponse = await SendAuthorizedJsonAsync(
            HttpMethod.Post,
            $"/api/games/{created!.GameId}/join-online",
            greenToken);
        joinResponse.EnsureSuccessStatusCode();

        return created.GameId;
    }

    private async Task<GameResponse> GetGameAsync(Guid gameId, string token)
    {
        var response = await SendAuthorizedJsonAsync(
            HttpMethod.Get,
            $"/api/games/{gameId}",
            token);
        response.EnsureSuccessStatusCode();

        var state = await response.Content.ReadFromJsonAsync<GameResponse>();
        Assert.NotNull(state);
        return state!;
    }

    private static UserEntity CreateUser(string prefix)
    {
        return new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = $"{prefix}_{Guid.NewGuid()}@mail.com",
            Nickname = prefix,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(ValidPassword),
            Role = 0,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
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

    private async Task<HttpResponseMessage> SendAuthorizedJsonAsync(
        HttpMethod method,
        string url,
        string token,
        object? body = null)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (body is not null)
            request.Content = JsonContent.Create(body);

        return await _client.SendAsync(request);
    }
}
