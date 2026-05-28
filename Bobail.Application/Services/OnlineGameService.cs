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
    private readonly IOnlineGameClockService _clockService;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<OnlineGameService> _logger;

    public OnlineGameService(
        IGameRepository gameRepository,
        IGameStateRepository gameStateRepository,
        IGamePlayerRepository gamePlayerRepository,
        IGameLockManager gameLockManager,
        IOnlineGameClockService clockService,
        TimeProvider timeProvider,
        ILogger<OnlineGameService> logger)
    {
        _gameRepository = gameRepository;
        _gameStateRepository = gameStateRepository;
        _gamePlayerRepository = gamePlayerRepository;
        _gameLockManager = gameLockManager;
        _clockService = clockService;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Guid> CreateOnlineGameAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var lockedGameId = await GetActiveOnlineGameIdForUserAsync(
            userId,
            cancellationToken);

        if (lockedGameId.HasValue)
            throw new DomainException("You are already in an active online game.");

        await AbandonWaitingOnlineGamesForUserAsync(
            userId,
            excludedGameId: null,
            cancellationToken);

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

    public async Task<Guid?> GetActiveOnlineGameIdForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var gameIds = await _gamePlayerRepository.GetActiveOnlineGameIdsForUserAsync(
            userId,
            cancellationToken);

        foreach (var gameId in gameIds)
        {
            using var gameLock = await _gameLockManager.AcquireAsync(
                gameId,
                cancellationToken);

            var game = await _gameRepository.GetByIdAsync(gameId, cancellationToken);

            if (game?.Status != GameStatus.InProgress)
                continue;

            if (await RefreshClockAsync(game, GetUtcNow(), cancellationToken))
                continue;

            if (game.Status == GameStatus.InProgress)
                return gameId;
        }

        return null;
    }

    public async Task<GameResponse> JoinOnlineGameAsync(
        Guid gameId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var lockedGameId = await GetActiveOnlineGameIdForUserAsync(
            userId,
            cancellationToken);

        if (lockedGameId.HasValue && lockedGameId.Value != gameId)
            throw new DomainException("You are already in an active online game.");

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

            await AbandonWaitingOnlineGamesForUserAsync(
                userId,
                gameId,
                cancellationToken);

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
            _clockService.StartClock(game, GetUtcNow());
            await _gameRepository.UpdateAsync(game, cancellationToken);
        }

        return GameResponseMapper.ToResponse(game, playerColor, GetUtcNow());
    }

    public async Task<GameResponse> GetGameStateForUserAsync(
        Guid gameId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var gameLock = await _gameLockManager.AcquireAsync(gameId, cancellationToken);

        var game = await GetOnlineGameAsync(gameId, cancellationToken);
        var playerColor = await RequirePlayerColorAsync(gameId, userId, cancellationToken);

        if (await RefreshClockAsync(game, GetUtcNow(), cancellationToken))
            game = await GetOnlineGameAsync(gameId, cancellationToken);

        return GameResponseMapper.ToResponse(game, playerColor, GetUtcNow());
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
        var now = GetUtcNow();
        var timeoutResult = await FinishIfTimedOutAsync(
            game,
            "PlayerMove",
            now,
            cancellationToken);

        if (timeoutResult is not null)
            return timeoutResult;

        EnsureCanMove(game, playerColor);
        var movingPlayer = game.CurrentTurn;

        game.ExecutePlayerMove(
            new Position(request.FromRow, request.FromColumn),
            new Position(request.ToRow, request.ToColumn));

        _clockService.CommitSuccessfulMove(game, movingPlayer, now);

        await PersistStateChangeAsync(game, cancellationToken);

        _logger.LogInformation(
            "Online player move. GameId: {GameId}, UserId: {UserId}, Color: {PlayerColor}",
            gameId,
            userId,
            playerColor);

        return ToMoveResult("PlayerMove", playerColor, game, now);
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
        var now = GetUtcNow();
        var timeoutResult = await FinishIfTimedOutAsync(
            game,
            "BobailMove",
            now,
            cancellationToken);

        if (timeoutResult is not null)
            return timeoutResult;

        EnsureCanMove(game, playerColor);
        var movingPlayer = game.CurrentTurn;

        game.ExecuteBobailMove(new Position(request.ToRow, request.ToColumn));

        _clockService.CommitSuccessfulMove(game, movingPlayer, now);

        await PersistStateChangeAsync(game, cancellationToken);

        _logger.LogInformation(
            "Online Bobail move. GameId: {GameId}, UserId: {UserId}, Color: {PlayerColor}",
            gameId,
            userId,
            playerColor);

        return ToMoveResult("BobailMove", playerColor, game, now);
    }

    public async Task<GameResponse> ResignGameAsync(
        Guid gameId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var gameLock = await _gameLockManager.AcquireAsync(gameId, cancellationToken);

        var game = await GetOnlineGameAsync(gameId, cancellationToken);
        var playerColor = await RequirePlayerColorAsync(gameId, userId, cancellationToken);
        var now = GetUtcNow();
        var timeoutResult = await FinishIfTimedOutAsync(
            game,
            "Resign",
            now,
            cancellationToken);

        if (timeoutResult is not null)
            return timeoutResult.Game;

        EnsureCanResign(game, playerColor);

        game.Finish(
            GetOpponentColor(playerColor),
            GameEndReason.Resignation);

        await PersistStateChangeAsync(game, cancellationToken);

        _logger.LogInformation(
            "Online player resigned. GameId: {GameId}, UserId: {UserId}, Color: {PlayerColor}",
            gameId,
            userId,
            playerColor);

        return GameResponseMapper.ToResponse(game, playerColor, now);
    }

    public async Task<IReadOnlyList<GameResponse>> ForfeitActiveGamesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default,
        GameEndReason endReason = GameEndReason.Forfeit)
    {
        var gameIds = await _gamePlayerRepository.GetActiveOnlineGameIdsForUserAsync(
            userId,
            cancellationToken);
        var finishedGames = new List<GameResponse>();

        foreach (var gameId in gameIds)
        {
            using var gameLock = await _gameLockManager.AcquireAsync(gameId, cancellationToken);

            var game = await GetOnlineGameAsync(gameId, cancellationToken);

            if (game.Status is not (GameStatus.WaitingForPlayers or GameStatus.InProgress))
                continue;

            var forfeitingColor = await _gamePlayerRepository.GetPlayerColorAsync(
                gameId,
                userId,
                cancellationToken);

            if (!forfeitingColor.HasValue)
                continue;

            var humanPlayerCount = await _gamePlayerRepository.CountHumanPlayersAsync(
                gameId,
                cancellationToken);

            if (humanPlayerCount < 2)
            {
                game.Abandon();
                await PersistStateChangeAsync(game, cancellationToken);
                continue;
            }

            var winner = GetOpponentColor(forfeitingColor.Value);
            game.Finish(winner, endReason);

            await PersistStateChangeAsync(game, cancellationToken);
            finishedGames.Add(GameResponseMapper.ToResponse(game, serverTimeUtc: GetUtcNow()));
        }

        return finishedGames;
    }

    public async Task<IReadOnlyList<GameResponse>> ExpireTimedOutGamesAsync(
        CancellationToken cancellationToken = default)
    {
        var gameIds = await _gameRepository.GetInProgressOnlineGameIdsAsync(cancellationToken);
        var expiredGames = new List<GameResponse>();

        foreach (var gameId in gameIds)
        {
            using var gameLock = await _gameLockManager.AcquireAsync(gameId, cancellationToken);
            var game = await GetOnlineGameAsync(gameId, cancellationToken);
            var now = GetUtcNow();

            if (game.Clock is null)
            {
                _clockService.StartClock(game, now);
                await PersistStateChangeAsync(game, cancellationToken);
                continue;
            }

            if (!_clockService.FinishIfTimedOut(game, now))
                continue;

            await PersistStateChangeAsync(game, cancellationToken);
            expiredGames.Add(GameResponseMapper.ToResponse(game, serverTimeUtc: now));

            _logger.LogInformation(
                "Online game expired on time. GameId: {GameId}, Winner: {Winner}",
                game.Id,
                game.Winner);
        }

        return expiredGames;
    }

    private async Task AbandonWaitingOnlineGamesForUserAsync(
        Guid userId,
        Guid? excludedGameId,
        CancellationToken cancellationToken)
    {
        var gameIds = await _gamePlayerRepository.GetActiveOnlineGameIdsForUserAsync(
            userId,
            cancellationToken);

        foreach (var gameId in gameIds)
        {
            if (excludedGameId.HasValue && gameId == excludedGameId.Value)
                continue;

            using var gameLock = await _gameLockManager.AcquireAsync(gameId, cancellationToken);
            var game = await GetOnlineGameAsync(gameId, cancellationToken);

            if (game.Status != GameStatus.WaitingForPlayers)
                continue;

            game.Abandon();
            await PersistStateChangeAsync(game, cancellationToken);
        }
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

    private static void EnsureCanResign(Game game, PlayerColor playerColor)
    {
        if (game.Status != GameStatus.InProgress)
            throw new DomainException("Game is not active.");

        if (game.CurrentTurn != playerColor)
            throw new DomainException("You can resign only on your turn.");
    }

    private async Task<bool> RefreshClockAsync(
        Game game,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (game.Status != GameStatus.InProgress)
            return false;

        if (game.Clock is null)
        {
            _clockService.StartClock(game, now);
            await PersistStateChangeAsync(game, cancellationToken);
            return false;
        }

        if (!_clockService.FinishIfTimedOut(game, now))
            return false;

        await PersistStateChangeAsync(game, cancellationToken);
        return true;
    }

    private async Task<OnlineGameMoveResult?> FinishIfTimedOutAsync(
        Game game,
        string requestedMoveType,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var timedOutPlayer = _clockService.GetTimedOutPlayer(game, now);

        if (!timedOutPlayer.HasValue)
            return null;

        _clockService.FinishIfTimedOut(game, now);
        await PersistStateChangeAsync(game, cancellationToken);

        _logger.LogInformation(
            "Online move rejected because clock expired. GameId: {GameId}, RequestedMoveType: {MoveType}, TimedOutPlayer: {PlayerColor}",
            game.Id,
            requestedMoveType,
            timedOutPlayer);

        return ToMoveResult("Timeout", timedOutPlayer.Value, game, now);
    }

    private async Task PersistStateChangeAsync(
        Game game,
        CancellationToken cancellationToken)
    {
        await _gameRepository.UpdateAsync(game, cancellationToken);
        await _gameStateRepository.AddSnapshotAsync(game, cancellationToken);
    }

    private static OnlineGameMoveResult ToMoveResult(
        string moveType,
        PlayerColor playerColor,
        Game game,
        DateTimeOffset now)
    {
        return new OnlineGameMoveResult
        {
            MoveType = moveType,
            PlayerColor = playerColor.ToString(),
            Game = GameResponseMapper.ToResponse(game, serverTimeUtc: now)
        };
    }

    private DateTimeOffset GetUtcNow()
    {
        return _timeProvider.GetUtcNow().ToUniversalTime();
    }

    private static PlayerColor GetOpponentColor(PlayerColor playerColor)
    {
        return playerColor == PlayerColor.Red
            ? PlayerColor.Green
            : PlayerColor.Red;
    }
}
