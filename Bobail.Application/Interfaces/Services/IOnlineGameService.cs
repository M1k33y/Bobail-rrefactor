using Bobail.Application.DTOs;

namespace Bobail.Application.Interfaces.Services;

public interface IOnlineGameService
{
    Task<Guid> CreateOnlineGameAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Guid?> GetActiveOnlineGameIdForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<GameResponse> JoinOnlineGameAsync(
        Guid gameId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<GameResponse> GetGameStateForUserAsync(
        Guid gameId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<OnlineGameMoveResult> ExecutePlayerMoveAsync(
        Guid gameId,
        Guid userId,
        PlayerMoveRequest request,
        CancellationToken cancellationToken = default);

    Task<OnlineGameMoveResult> ExecuteBobailMoveAsync(
        Guid gameId,
        Guid userId,
        BobailMoveRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GameResponse>> ForfeitActiveGamesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
