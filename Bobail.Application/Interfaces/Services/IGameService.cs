using Bobail.Application.DTOs;
using Bobail.Domain.Games;

namespace Bobail.Application.Interfaces.Services;

public interface IGameService
{
    Task<Guid> CreateGameAsync(
        GameMode mode,
        BotDifficulty? difficulty,
        PlayerColor? botColor,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateGameAsync(
        CancellationToken cancellationToken = default);

    Task<Game> GetGameAsync(
        Guid gameId,
        CancellationToken cancellationToken = default);

    Task ExecutePlayerMoveAsync(
        Guid gameId,
        int fromRow,
        int fromColumn,
        int toRow,
        int toColumn,
        CancellationToken cancellationToken = default);

    Task ExecuteBobailMoveAsync(
        Guid gameId,
        int toRow,
        int toColumn,
        CancellationToken cancellationToken = default);

    Task<List<(int row, int col)>> GetValidPlayerMovesAsync(
        Guid gameId,
        int row,
        int col);

    Task ExecuteBotCycleAsync(
        Guid gameId,
        CancellationToken cancellationToken);

    Task AbandonGameAsync(Guid gameId);

    Task<PagedGameHistoryResponse> GetHistoryForUserAsync(
        Guid userId,
        GameHistoryQuery query,
        CancellationToken cancellationToken = default);

    Task<UserGameStatsResponse> GetUserStatsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<GameReplayResponse> GetReplayAsync(
        Guid gameId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
