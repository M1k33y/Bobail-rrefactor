using Bobail.Application.Interfaces.Repositories;
using Bobail.Domain.Games;
using Bobail.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class SqlGamePlayerRepository : IGamePlayerRepository
{
    private readonly GameDbContext _context;

    public SqlGamePlayerRepository(GameDbContext context)
    {
        _context = context;
    }

    public async Task AddPlayersForGame(Guid gameId, Guid userId, bool isVsBot, PlayerColor? botColor = null)
    {
        var players = new List<GamePlayerEntity>();

        if (isVsBot)
        {
            var resolvedBotColor = botColor ?? PlayerColor.Green;
            var userColor = resolvedBotColor == PlayerColor.Red
                ? PlayerColor.Green
                : PlayerColor.Red;

            players.Add(new GamePlayerEntity
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                UserId = userId,
                Color = (int)userColor,
                IsBot = false
            });

            players.Add(new GamePlayerEntity
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                UserId = null,
                Color = (int)resolvedBotColor,
                IsBot = true
            });
        }
        else
        {
            players.Add(new GamePlayerEntity
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                UserId = userId,
                Color = (int)PlayerColor.Red,
                IsBot = false
            });

            // future multiplayer player mapping will be added here
        }

        _context.GamePlayers.AddRange(players);
        await _context.SaveChangesAsync();
    }

    public Task<bool> UserParticipatesInGameAsync(Guid gameId, Guid userId)
    {
        return _context.GamePlayers.AnyAsync(x => x.GameId == gameId && x.UserId == userId);
    }
}
