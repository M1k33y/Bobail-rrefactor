using Bobail.Application.Interfaces.Repositories;
using Bobail.Application.Interfaces.Services;
using Bobail.Domain.Board;
using Bobail.Domain.Common;
using Bobail.Domain.Games;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Bobail.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _repository;
    private readonly IBotService _botService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GameService> _logger;

    public GameService(
    IGameRepository repository,
    IBotService botService,
    ILogger<GameService> logger,
    IServiceScopeFactory scopeFactory)
    {
        _repository = repository;
        _botService = botService;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task<Guid> CreateGameAsync(
        GameMode mode,
        BotDifficulty? difficulty,
        PlayerColor? botColor,
        CancellationToken cancellationToken = default)
    {
        var game = new Game(mode, difficulty, botColor);

        await _repository.AddAsync(game, cancellationToken);

        _logger.LogInformation(
            "Game created. Id: {GameId}, Mode: {Mode}, Difficulty: {Difficulty}",
            game.Id, mode, difficulty);

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
        var game = await _repository.GetByIdAsync(gameId, cancellationToken);

        if (game is null)
        {
            _logger.LogWarning("Game not found. Id: {GameId}", gameId);
            throw new DomainException("Game not found.");
        }

        return game;
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
        Console.WriteLine($"Game instance: {game.GetHashCode()}");
        _logger.LogInformation(
            "Player move. GameId: {GameId}, From: ({FromRow},{FromCol}) -> To: ({ToRow},{ToCol})",
            gameId, fromRow, fromColumn, toRow, toColumn);

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

        _logger.LogInformation(
            "Bobail move. GameId: {GameId}, Target: ({Row},{Col})",
            gameId, toRow, toColumn);

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

        _logger.LogInformation(
            "Bot triggered. GameId: {GameId}",
            game.Id);

        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();

            var gameService = scope.ServiceProvider
                .GetRequiredService<IGameService>();

            try
            {
                await gameService.ExecuteBotCycleAsync(
                    game.Id,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error during bot cycle. GameId: {GameId}",
                    game.Id);
            }
        });
    }

   public async Task ExecuteBotCycleAsync(
        Guid gameId,
        CancellationToken cancellationToken)
    {
        var game = await _repository
            .GetByIdAsync(gameId, cancellationToken);

        if (game is null ||
            !game.IsBotTurn() ||
            game.Status != GameStatus.InProgress)
            return;

        _logger.LogInformation(
            "Bot cycle started. GameId: {GameId}",
            gameId);

        await _botService.ExecuteSingleMoveAsync(game);
        await _repository.UpdateAsync(game, cancellationToken);

        _logger.LogInformation(
            "Bot first move executed. GameId: {GameId}",
            gameId);

        if (game.IsBotTurn() &&
            game.Status == GameStatus.InProgress)
        {

            await _botService.ExecuteSingleMoveAsync(game);
            await _repository.UpdateAsync(game, cancellationToken);

            _logger.LogInformation(
                "Bot second move executed. GameId: {GameId}",
                gameId);
        }
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
}