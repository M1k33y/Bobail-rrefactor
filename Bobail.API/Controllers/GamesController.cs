using Microsoft.AspNetCore.Mvc;
using Bobail.Application.Services;
using Bobail.API.DTOs;
using Bobail.Domain.Games;

namespace Bobail.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly GameService _gameService;

    public GamesController(GameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost]
    public async Task<ActionResult> CreateGame()
    {
        var gameId = await _gameService.CreateGameAsync();

        return Ok(new { gameId });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetGame(Guid id)
    {
        var game = await _gameService.GetGameAsync(id);

        return Ok(new
        {
            game.Id,
            game.Status,
            game.CurrentTurn,
            game.Winner,
            game.IsFirstTurn,
            game.CurrentPhase,
            Pieces = game.Board.Pieces.Select(p => new
            {
                p.Type,
                p.Position.Row,
                p.Position.Column
            })
        });
    }

    [HttpPost("{id:guid}/bobail-move")]
    public async Task<ActionResult> ExecuteBobailMove(Guid id, [FromBody] BobailMoveRequest request)
    {
        await _gameService.ExecuteBobailMoveAsync(
            id,
            request.ToRow,
            request.ToColumn);

        return NoContent();
    }



    [HttpPost("{id:guid}/player-move")]
    public async Task<ActionResult> ExecutePlayerMove(Guid id, [FromBody] PlayerMoveRequest request)
    {
        await _gameService.ExecutePlayerMoveAsync(
            id,
            request.FromRow,
            request.FromColumn,
            request.ToRow,
            request.ToColumn);

        return NoContent();
    }

    [HttpGet("{id:guid}/valid-player-moves")]
    public async Task<ActionResult> GetValidPlayerMoves(Guid id, int row, int col)
    {
        var moves = await _gameService.GetValidPlayerMovesAsync(id, row, col);

        return Ok(moves.Select(m => new { row = m.row, column = m.col }));
    }

    [HttpGet("{id:guid}/valid-bobail-moves")]
    public async Task<ActionResult> GetValidBobailMoves(Guid id)
    {
        var game = await _gameService.GetGameAsync(id);

        var moves = game.GetValidBobailMoves();

        return Ok(moves.Select(m => new { row = m.Row, column = m.Column }));
    }



}
