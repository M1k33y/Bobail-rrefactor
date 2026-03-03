using Bobail.Application.Interfaces.Repositories;
using Bobail.Application.Interfaces.Services;
using Bobail.Domain.Board;
using Bobail.Domain.Common;
using Bobail.Domain.Games;

namespace Bobail.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _repository;
    private readonly IBotService _botService;

    public GameService(
        IGameRepository repository,
        IBotService botService)
    {
        _repository = repository;
        _botService = botService;
    }

 

    public async Task<Guid> CreateGameAsync(
        GameMode mode,
        BotDifficulty? difficulty,
        PlayerColor? botColor,
        CancellationToken cancellationToken = default)
    {
        var game = new Game(mode, difficulty, botColor);

        await _repository.AddAsync(game, cancellationToken);

        TriggerBotIfNeeded(game);

        return game.Id;
    }

    public Task<Guid> CreateGameAsync(
        CancellationToken cancellationToken = default)
    {
        return CreateGameAsync(
            GameMode.LocalMultiplayer,
            null,
            null,
            cancellationToken);
    }

   

    public async Task<Game> GetGameAsync(
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        var game = await _repository
            .GetByIdAsync(gameId, cancellationToken);

        if (game is null)
            throw new DomainException("Game not found.");

        return game;
    }

    public async Task<List<(int row, int col)>> GetValidPlayerMovesAsync(
        Guid gameId,
        int row,
        int col)
    {
        var game = await GetGameAsync(gameId);

        var moves = game.GetValidPlayerMoves(
            new Position(row, col));

        return moves
            .Select(m => (m.Row, m.Column))
            .ToList();
    }

  

    public async Task ExecutePlayerMoveAsync(
        Guid gameId,
        int fromRow,
        int fromColumn,
        int toRow,
        int toColumn,
        CancellationToken cancellationToken = default)
    {
        var game = await GetGameAsync(gameId, cancellationToken);

        var from = new Position(fromRow, fromColumn);
        var to = new Position(toRow, toColumn);

        game.ExecutePlayerMove(from, to);

        await _repository.UpdateAsync(game, cancellationToken);

        TriggerBotIfNeeded(game);
    }

    public async Task ExecuteBobailMoveAsync(
        Guid gameId,
        int toRow,
        int toColumn,
        CancellationToken cancellationToken = default)
    {
        var game = await GetGameAsync(gameId, cancellationToken);

        var target = new Position(toRow, toColumn);

        game.ExecuteBobailMove(target);

        await _repository.UpdateAsync(game, cancellationToken);

        TriggerBotIfNeeded(game);
    }

    

    private void TriggerBotIfNeeded(Game game)
    {
        if (!game.IsBotTurn() ||
            game.Status != GameStatus.InProgress)
            return;

        _ = Task.Run(async () =>
        {
            await ExecuteBotCycleAsync(
                game.Id,
                CancellationToken.None);
        });
    }

    private async Task ExecuteBotCycleAsync(
        Guid gameId,
        CancellationToken cancellationToken)
    {
        var game = await _repository
            .GetByIdAsync(gameId, cancellationToken);

        if (game is null ||
            !game.IsBotTurn() ||
            game.Status != GameStatus.InProgress)
            return;

       
        await _botService.ExecuteSingleMoveAsync(game);
        await _repository.UpdateAsync(game, cancellationToken);

        
        if (game.IsBotTurn() &&
            game.Status == GameStatus.InProgress)
        {
            await Task.Delay(600, cancellationToken);

            await _botService.ExecuteSingleMoveAsync(game);
            await _repository.UpdateAsync(game, cancellationToken);
        }
    }
}