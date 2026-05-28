using Bobail.API.Extensions;
using Bobail.API.Realtime;
using Bobail.Application.DTOs;
using Bobail.Application.Interfaces.Repositories;
using Bobail.Application.Interfaces.Services;
using Bobail.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Bobail.API.Hubs;

[Authorize]
public class GameHub : Hub
{
    private readonly IOnlineGameService _onlineGameService;
    private readonly IGameConnectionTracker _connectionTracker;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GameHub> _logger;

    public GameHub(
        IOnlineGameService onlineGameService,
        IGameConnectionTracker connectionTracker,
        IUserRepository userRepository,
        ILogger<GameHub> logger)
    {
        _onlineGameService = onlineGameService;
        _connectionTracker = connectionTracker;
        _userRepository = userRepository;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = await GetActiveUserIdOrDisconnectAsync();

        if (!userId.HasValue)
            return;

        await _connectionTracker.TrackConnectionAsync(
            Context.ConnectionId,
            userId.Value);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _connectionTracker.RemoveConnectionAsync(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGame(string gameId)
    {
        var userId = await GetActiveUserIdOrDisconnectAsync();

        if (!userId.HasValue)
            return;

        if (!TryParseGameId(gameId, out var parsedGameId))
        {
            await SendErrorAsync("JoinRejected", "Invalid game id.");
            return;
        }

        try
        {
            var state = await _onlineGameService.GetGameStateForUserAsync(
                parsedGameId,
                userId.Value,
                Context.ConnectionAborted);
            var groupName = GetGroupName(parsedGameId);

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                groupName,
                Context.ConnectionAborted);

            await _connectionTracker.TrackGameConnectionAsync(
                Context.ConnectionId,
                parsedGameId);

            await Clients.Caller.SendAsync(
                "GameState",
                state,
                Context.ConnectionAborted);

            await Clients.GroupExcept(groupName, Context.ConnectionId).SendAsync(
                "PlayerJoined",
                WithoutPlayerColor(state),
                Context.ConnectionAborted);
        }
        catch (Exception ex) when (IsExpectedClientError(ex))
        {
            await SendErrorAsync("JoinRejected", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while joining game. GameId: {GameId}",
                gameId);

            await SendErrorAsync("JoinRejected", "Unable to join game.");
        }
    }

    public async Task MakePlayerMove(string gameId, PlayerMoveRequest request)
    {
        if (!await EnsureActiveUserAsync())
            return;

        await ExecuteMoveAsync(
            gameId,
            "PlayerMove",
            parsedGameId => _onlineGameService.ExecutePlayerMoveAsync(
                parsedGameId,
                Context.User!.GetUserId(),
                request,
                Context.ConnectionAborted));
    }

    public async Task MakeBobailMove(string gameId, BobailMoveRequest request)
    {
        if (!await EnsureActiveUserAsync())
            return;

        await ExecuteMoveAsync(
            gameId,
            "BobailMove",
            parsedGameId => _onlineGameService.ExecuteBobailMoveAsync(
                parsedGameId,
                Context.User!.GetUserId(),
                request,
                Context.ConnectionAborted));
    }

    public async Task ResignGame(string gameId)
    {
        if (!await EnsureActiveUserAsync())
            return;

        if (!TryParseGameId(gameId, out var parsedGameId))
        {
            await SendErrorAsync("ResignRejected", "Invalid game id.");
            return;
        }

        try
        {
            var game = await _onlineGameService.ResignGameAsync(
                parsedGameId,
                Context.User!.GetUserId(),
                Context.ConnectionAborted);
            var groupName = GetGroupName(parsedGameId);

            await Clients.Group(groupName).SendAsync(
                "GameEnded",
                game,
                Context.ConnectionAborted);
        }
        catch (Exception ex) when (IsExpectedClientError(ex))
        {
            await SendErrorAsync("ResignRejected", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while resigning game. GameId: {GameId}",
                gameId);

            await SendErrorAsync("ResignRejected", "Unable to resign game.");
        }
    }

    private async Task ExecuteMoveAsync(
        string gameId,
        string moveType,
        Func<Guid, Task<OnlineGameMoveResult>> executeMove)
    {
        if (!TryParseGameId(gameId, out var parsedGameId))
        {
            await SendErrorAsync("MoveRejected", "Invalid game id.");
            return;
        }

        try
        {
            var result = await executeMove(parsedGameId);
            var groupName = GetGroupName(parsedGameId);

            await Clients.Group(groupName).SendAsync(
                "MovePlayed",
                result,
                Context.ConnectionAborted);

            if (result.Game.Status == "Finished")
            {
                await Clients.Group(groupName).SendAsync(
                    "GameEnded",
                    result.Game,
                    Context.ConnectionAborted);
            }
        }
        catch (Exception ex) when (IsExpectedClientError(ex))
        {
            await SendErrorAsync("MoveRejected", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while processing {MoveType}. GameId: {GameId}",
                moveType,
                gameId);

            await SendErrorAsync("MoveRejected", "Unable to process move.");
        }
    }

    private Task SendErrorAsync(string eventName, string message)
    {
        return Clients.Caller.SendAsync(
            eventName,
            new { message },
            Context.ConnectionAborted);
    }

    private async Task<bool> EnsureActiveUserAsync()
    {
        return (await GetActiveUserIdOrDisconnectAsync()).HasValue;
    }

    private async Task<Guid?> GetActiveUserIdOrDisconnectAsync()
    {
        var userId = Context.User!.GetUserId();
        var user = await _userRepository.GetByIdAsync(userId);

        if (user is not null && user.IsActive)
            return userId;

        await Clients.Caller.SendAsync(
            "ForceLogout",
            new { message = "Your account has been banned." },
            Context.ConnectionAborted);

        Context.Abort();
        return null;
    }

    private static bool TryParseGameId(string gameId, out Guid parsedGameId)
    {
        return Guid.TryParse(gameId, out parsedGameId);
    }

    private static string GetGroupName(Guid gameId)
    {
        return gameId.ToString("D");
    }

    private static GameResponse WithoutPlayerColor(GameResponse state)
    {
        return new GameResponse
        {
            Id = state.Id,
            Status = state.Status,
            CurrentTurn = state.CurrentTurn,
            Winner = state.Winner,
            EndReason = state.EndReason,
            IsFirstTurn = state.IsFirstTurn,
            CurrentPhase = state.CurrentPhase,
            Mode = state.Mode,
            BotColor = state.BotColor,
            Clock = state.Clock,
            Pieces = state.Pieces.Select(p => new PieceDto
            {
                Type = p.Type,
                Owner = p.Owner,
                Row = p.Row,
                Column = p.Column
            }).ToList()
        };
    }

    private static bool IsExpectedClientError(Exception ex)
    {
        return ex is DomainException
            || ex is InvalidOperationException
            || ex is UnauthorizedAccessException;
    }
}
