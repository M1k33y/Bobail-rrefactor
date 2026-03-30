using Bobail.Application.DTOs;
using Bobail.Application.Interfaces.Services;
using Bobail.Domain.Games;
using Microsoft.AspNetCore.Mvc;

namespace Bobail.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }


    [HttpPost]
    public async Task<ActionResult> CreateGame()
    {
        var gameId = await _gameService.CreateGameAsync();

        return Ok(new CreateGameResponse
        {
            GameId = gameId
        });
    }

    [HttpPost("vs-bot")]
    public async Task<ActionResult> CreateGameVsBot(
        [FromBody] CreateBotGameRequest request)
    {
        var gameId = await _gameService.CreateGameAsync(
        GameMode.PlayerVsBot,
        request.Difficulty,
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

        var response = new GameResponse
        {
            Id = game.Id,
            Status = game.Status.ToString(),
            CurrentTurn = game.CurrentTurn.ToString(),
            Winner = game.Winner?.ToString(),
            IsFirstTurn = game.IsFirstTurn,
            CurrentPhase = game.CurrentPhase.ToString(),
            Mode = game.Mode.ToString(),
            BotColor = game.BotColor?.ToString(),   

            Pieces = game.Board.Pieces.Select(p => new PieceDto
            {
                Type = p.Type.ToString(),
                Owner = p.Owner?.ToString(),
                Row = p.Position.Row,
                Column = p.Position.Column
            }).ToList()
        };

        return Ok(response);
    }


    [HttpPost("{id:guid}/player-move")]
    public async Task<ActionResult> ExecutePlayerMove(
        Guid id,
        [FromBody] PlayerMoveRequest request)
    {
        await _gameService.ExecutePlayerMoveAsync(
            id,
            request.FromRow,
            request.FromColumn,
            request.ToRow,
            request.ToColumn);

        return NoContent();
    }


    [HttpPost("{id:guid}/bobail-move")]
    public async Task<ActionResult> ExecuteBobailMove(
        Guid id,
        [FromBody] BobailMoveRequest request)
    {
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
}