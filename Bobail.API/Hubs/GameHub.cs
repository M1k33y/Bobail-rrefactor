using Bobail.API.Extensions;
using Bobail.API.Realtime;
using Bobail.Application.DTOs;
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
    private readonly ILogger<GameHub> _logger;

    public GameHub(
        IOnlineGameService onlineGameService,
        IGameConnectionTracker connectionTracker,
        ILogger<GameHub> logger)
    {
        _onlineGameService = onlineGameService;
        _connectionTracker = connectionTracker;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        await _connectionTracker.TrackConnectionAsync(
            Context.ConnectionId,
            Context.User!.GetUserId());

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _connectionTracker.RemoveConnectionAsync(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGame(string gameId)
    {
        if (!TryParseGameId(gameId, out var parsedGameId))
        {
            await SendErrorAsync("JoinRejected", "Invalid game id.");
            return;
        }

        try
        {
            var userId = Context.User!.GetUserId();
            var state = await _onlineGameService.GetGameStateForUserAsync(
                parsedGameId,
                userId,
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
        await ExecuteMoveAsync(
            gameId,
            "BobailMove",
            parsedGameId => _onlineGameService.ExecuteBobailMoveAsync(
                parsedGameId,
                Context.User!.GetUserId(),
                request,
                Context.ConnectionAborted));
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
            IsFirstTurn = state.IsFirstTurn,
            CurrentPhase = state.CurrentPhase,
            Mode = state.Mode,
            BotColor = state.BotColor,
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
