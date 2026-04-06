using Bobail.Domain.Games;

namespace Bobail.Application.Interfaces.Repositories;

public interface IGamePlayerRepository
{
    Task AddPlayersForGame(Guid gameId, Guid userId, bool isVsBot, PlayerColor? botColor = null);
    Task<bool> UserParticipatesInGameAsync(Guid gameId, Guid userId);
}
