using Bobail.API.Extensions;
using Bobail.Application.DTOs;
using Bobail.Application.Interfaces.Repositories;
using Bobail.Application.Interfaces.Services;
using Bobail.Domain.Games;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bobail.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly IOnlineGameService _onlineGameService;
    private readonly IGamePlayerRepository _gamePlayerRepository;


    public GamesController(
     IGameService gameService,
     IOnlineGameService onlineGameService,
     IGamePlayerRepository gamePlayerRepository)
    {
        _gameService = gameService;
        _onlineGameService = onlineGameService;
        _gamePlayerRepository = gamePlayerRepository;
    }


    [Authorize]
    [HttpPost]
    public async Task<ActionResult> CreateGame()
    {
        var userId = User.GetUserId();

        var gameId = await _gameService.CreateGameAsync();

        await _gamePlayerRepository.AddPlayersForGame(
            gameId,
            userId,
            false);

        return Ok(new CreateGameResponse
        {
            GameId = gameId
        });
    }

    [Authorize]
    [HttpPost("online")]
    public async Task<ActionResult> CreateOnlineGame(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var gameId = await _onlineGameService.CreateOnlineGameAsync(
            userId,
            cancellationToken);

        return Ok(new CreateGameResponse
        {
            GameId = gameId
        });
    }

    [Authorize]
    [HttpGet("online/current")]
    public async Task<ActionResult<ActiveOnlineGameResponse>> GetCurrentOnlineGame(
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var gameId = await _onlineGameService.GetActiveOnlineGameIdForUserAsync(
            userId,
            cancellationToken);

        return Ok(new ActiveOnlineGameResponse
        {
            GameId = gameId
        });
    }

    [Authorize]
    [HttpPost("{id:guid}/join-online")]
    public async Task<ActionResult<GameResponse>> JoinOnlineGame(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var state = await _onlineGameService.JoinOnlineGameAsync(
            id,
            userId,
            cancellationToken);

        return Ok(state);
    }

    [Authorize]
    [HttpPost("vs-bot")]
    public async Task<ActionResult> CreateGameVsBot(
     [FromBody] CreateBotGameRequest request)
    {
        var userId = User.GetUserId();

        var gameId = await _gameService.CreateGameAsync(
            GameMode.PlayerVsBot,
            request.Difficulty,
            request.BotColor);

        await _gamePlayerRepository.AddPlayersForGame(
            gameId,
            userId,
            true,
            request.BotColor);

        return Ok(new CreateGameResponse
        {
            GameId = gameId
        });
    }


    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetGame(Guid id)
    {
        var game = await _gameService.GetGameAsync(id);

        PlayerColor? playerColor = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            playerColor = await _gamePlayerRepository.GetPlayerColorAsync(
                id,
                User.GetUserId());
        }

        var response = GameResponseMapper.ToResponse(game, playerColor);

        return Ok(response);
    }

    [Authorize]
    [HttpPost("{id:guid}/player-move")]
    public async Task<ActionResult> ExecutePlayerMove(
        Guid id,
        [FromBody] PlayerMoveRequest request)
    {
        var game = await _gameService.GetGameAsync(id);

        if (game.Mode == GameMode.OnlineMultiplayer)
        {
            await _onlineGameService.ExecutePlayerMoveAsync(
                id,
                User.GetUserId(),
                request);

            return NoContent();
        }

        await _gameService.ExecutePlayerMoveAsync(
            id,
            request.FromRow,
            request.FromColumn,
            request.ToRow,
            request.ToColumn);

        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:guid}/bobail-move")]
    public async Task<ActionResult> ExecuteBobailMove(
        Guid id,
        [FromBody] BobailMoveRequest request)
    {
        var game = await _gameService.GetGameAsync(id);

        if (game.Mode == GameMode.OnlineMultiplayer)
        {
            await _onlineGameService.ExecuteBobailMoveAsync(
                id,
                User.GetUserId(),
                request);

            return NoContent();
        }

        await _gameService.ExecuteBobailMoveAsync(
            id,
            request.ToRow,
            request.ToColumn);

        return NoContent();
    }


    [HttpGet("{id:guid}/valid-player-moves")]
    public async Task<ActionResult> GetValidPlayerMoves(
        Guid id,
        int row,
        int col)
    {
        var moves = await _gameService.GetValidPlayerMovesAsync(id, row, col);

        return Ok(moves.Select(m => new
        {
            row = m.row,
            column = m.col
        }));
    }

    [HttpGet("{id:guid}/valid-bobail-moves")]
    public async Task<ActionResult> GetValidBobailMoves(Guid id)
    {
        var game = await _gameService.GetGameAsync(id);

        var moves = game.GetValidBobailMoves();

        return Ok(moves.Select(m => new
        {
            row = m.Row,
            column = m.Column
        }));
    }

    [Authorize]
    [HttpPost("{id}/abandon")]
    public async Task<IActionResult> AbandonGame(Guid id)
    {
        await _gameService.AbandonGameAsync(id);
        return NoContent();
    }

    [Authorize]
    [HttpGet("history")]
    public async Task<ActionResult> GetHistory(
        [FromQuery] GameHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var history = await _gameService.GetHistoryForUserAsync(userId, query, cancellationToken);
        return Ok(history);
    }

    [Authorize]
    [HttpGet("user-stats")]
    public async Task<ActionResult> GetUserStats(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var stats = await _gameService.GetUserStatsAsync(userId, cancellationToken);
        return Ok(stats);
    }

    [Authorize]
    [HttpGet("{id:guid}/replay")]
    public async Task<ActionResult> GetReplay(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var replay = await _gameService.GetReplayAsync(id, userId, cancellationToken);
        return Ok(replay);
    }


}
