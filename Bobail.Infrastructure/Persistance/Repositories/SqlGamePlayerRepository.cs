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
        var alreadyParticipant = await _context.GamePlayers
            .AnyAsync(x => x.GameId == gameId && x.UserId == userId);

        if (alreadyParticipant)
        {
            return;
        }

        var players = new List<GamePlayerEntity>();

        if (isVsBot)
        {
            var resolvedBotColor = botColor ?? PlayerColor.Green;
            var userColor = resolvedBotColor == PlayerColor.Red
                ? PlayerColor.Green
                : PlayerColor.Red;

            var userSlotTaken = await _context.GamePlayers
                .AnyAsync(x => x.GameId == gameId && x.Color == (int)userColor);

            if (!userSlotTaken)
            {
                players.Add(new GamePlayerEntity
                {
                    Id = Guid.NewGuid(),
                    GameId = gameId,
                    UserId = userId,
                    Color = (int)userColor,
                    IsBot = false
                });
            }

            var botSlotTaken = await _context.GamePlayers
                .AnyAsync(x => x.GameId == gameId && x.Color == (int)resolvedBotColor && x.IsBot);

            if (!botSlotTaken)
            {
                players.Add(new GamePlayerEntity
                {
                    Id = Guid.NewGuid(),
                    GameId = gameId,
                    UserId = null,
                    Color = (int)resolvedBotColor,
                    IsBot = true
                });
            }
        }
        else
        {
            var slotTaken = await _context.GamePlayers
                .AnyAsync(x => x.GameId == gameId && x.Color == (int)PlayerColor.Red);

            if (!slotTaken)
            {
                players.Add(new GamePlayerEntity
                {
                    Id = Guid.NewGuid(),
                    GameId = gameId,
                    UserId = userId,
                    Color = (int)PlayerColor.Red,
                    IsBot = false
                });
            }

            // future multiplayer player mapping will be added here
        }

        if (players.Count > 0)
        {
            _context.GamePlayers.AddRange(players);
            await _context.SaveChangesAsync();
        }
    }

    public Task<bool> UserParticipatesInGameAsync(Guid gameId, Guid userId)
    {
        return _context.GamePlayers.AnyAsync(x => x.GameId == gameId && x.UserId == userId);
    }
}
