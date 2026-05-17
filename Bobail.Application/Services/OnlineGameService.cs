using Bobail.Application.DTOs;
using Bobail.Application.Interfaces.Repositories;
using Bobail.Application.Interfaces.Services;
using Bobail.Domain.Board;
using Bobail.Domain.Common;
using Bobail.Domain.Games;
using Microsoft.Extensions.Logging;

namespace Bobail.Application.Services;

public class OnlineGameService : IOnlineGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameStateRepository _gameStateRepository;
    private readonly IGamePlayerRepository _gamePlayerRepository;
    private readonly IGameLockManager _gameLockManager;
    private readonly ILogger<OnlineGameService> _logger;

    public OnlineGameService(
        IGameRepository gameRepository,
        IGameStateRepository gameStateRepository,
        IGamePlayerRepository gamePlayerRepository,
        IGameLockManager gameLockManager,
        ILogger<OnlineGameService> logger)
    {
        _gameRepository = gameRepository;
        _gameStateRepository = gameStateRepository;
        _gamePlayerRepository = gamePlayerRepository;
        _gameLockManager = gameLockManager;
        _logger = logger;
    }

    public async Task<Guid> CreateOnlineGameAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var game = new Game(GameMode.OnlineMultiplayer);

        await _gameRepository.AddAsync(game, cancellationToken);
        await _gamePlayerRepository.AddOnlinePlayerAsync(game.Id, userId, cancellationToken);
        await _gameStateRepository.AddSnapshotAsync(game, cancellationToken);

        _logger.LogInformation(
            "Online game created. Id: {GameId}, OwnerUserId: {UserId}",
            game.Id,
            userId);

        return game.Id;
    }

    public async Task<GameResponse> JoinOnlineGameAsync(
        Guid gameId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var gameLock = await _gameLockManager.AcquireAsync(gameId, cancellationToken);

        var game = await GetOnlineGameAsync(gameId, cancellationToken);
        var playerColor = await _gamePlayerRepository.GetPlayerColorAsync(
            gameId,
            userId,
            cancellationToken);

        if (!playerColor.HasValue)
        {
            if (game.Status != GameStatus.WaitingForPlayers)
                throw new DomainException("Game is not accepting new players.");

            playerColor = await _gamePlayerRepository.AddOnlinePlayerAsync(
                gameId,
                userId,
                cancellationToken);
        }

        var playerCount = await _gamePlayerRepository.CountHumanPlayersAsync(
            gameId,
            cancellationToken);

        if (playerCount == 2 && game.Status == GameStatus.WaitingForPlayers)
        {
            game.Start();
            await _gameRepository.UpdateAsync(game, cancellationToken);
        }

        return GameResponseMapper.ToResponse(game, playerColor);
    }

    public async Task<GameResponse> GetGameStateForUserAsync(
        Guid gameId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var game = await GetOnlineGameAsync(gameId, cancellationToken);
        var playerColor = await RequirePlayerColorAsync(gameId, userId, cancellationToken);

        return GameResponseMapper.ToResponse(game, playerColor);
    }

    public async Task<OnlineGameMoveResult> ExecutePlayerMoveAsync(
        Guid gameId,
        Guid userId,
        PlayerMoveRequest request,
        CancellationToken cancellationToken = default)
    {
        using var gameLock = await _gameLockManager.AcquireAsync(gameId, cancellationToken);

        var game = await GetOnlineGameAsync(gameId, cancellationToken);
        var playerColor = await RequirePlayerColorAsync(gameId, userId, cancellationToken);

        EnsureCanMove(game, playerColor);

        game.ExecutePlayerMove(
            new Position(request.FromRow, request.FromColumn),
            new Position(request.ToRow, request.ToColumn));

        await PersistMoveAsync(game, cancellationToken);

        _logger.LogInformation(
            "Online player move. GameId: {GameId}, UserId: {UserId}, Color: {PlayerColor}",
            gameId,
            userId,
            playerColor);

        return ToMoveResult("PlayerMove", playerColor, game);
    }

    public async Task<OnlineGameMoveResult> ExecuteBobailMoveAsync(
        Guid gameId,
        Guid userId,
        BobailMoveRequest request,
        CancellationToken cancellationToken = default)
    {
        using var gameLock = await _gameLockManager.AcquireAsync(gameId, cancellationToken);

        var game = await GetOnlineGameAsync(gameId, cancellationToken);
        var playerColor = await RequirePlayerColorAsync(gameId, userId, cancellationToken);

        EnsureCanMove(game, playerColor);

        game.ExecuteBobailMove(new Position(request.ToRow, request.ToColumn));

        await PersistMoveAsync(game, cancellationToken);

        _logger.LogInformation(
            "Online Bobail move. GameId: {GameId}, UserId: {UserId}, Color: {PlayerColor}",
            gameId,
            userId,
            playerColor);

        return ToMoveResult("BobailMove", playerColor, game);
    }

    private async Task<Game> GetOnlineGameAsync(
        Guid gameId,
        CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, cancellationToken);

        if (game is null)
            throw new DomainException("Game not found.");

        if (game.Mode != GameMode.OnlineMultiplayer)
            throw new DomainException("Game is not an online multiplayer game.");

        return game;
    }

    private async Task<PlayerColor> RequirePlayerColorAsync(
        Guid gameId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var playerColor = await _gamePlayerRepository.GetPlayerColorAsync(
            gameId,
            userId,
            cancellationToken);

        if (!playerColor.HasValue)
            throw new DomainException("User does not belong to this game.");

        return playerColor.Value;
    }

    private static void EnsureCanMove(Game game, PlayerColor playerColor)
    {
        if (game.Status != GameStatus.InProgress)
            throw new DomainException("Game is not active.");

        if (game.CurrentTurn != playerColor)
            throw new DomainException("It is not your turn.");
    }

    private async Task PersistMoveAsync(
        Game game,
        CancellationToken cancellationToken)
    {
        await _gameRepository.UpdateAsync(game, cancellationToken);
        await _gameStateRepository.AddSnapshotAsync(game, cancellationToken);
    }

    private static OnlineGameMoveResult ToMoveResult(
        string moveType,
        PlayerColor playerColor,
        Game game)
    {
        return new OnlineGameMoveResult
        {
            MoveType = moveType,
            PlayerColor = playerColor.ToString(),
            Game = GameResponseMapper.ToResponse(game)
        };
    }
}
