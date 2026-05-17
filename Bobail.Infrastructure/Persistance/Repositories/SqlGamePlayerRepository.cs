using Bobail.Application.Interfaces.Repositories;
using Bobail.Domain.Common;
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

    public Task<bool> UserParticipatesInGameAsync(
        Guid gameId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _context.GamePlayers.AnyAsync(
            x => x.GameId == gameId && x.UserId == userId,
            cancellationToken);
    }

    public async Task<PlayerColor?> GetPlayerColorAsync(
        Guid gameId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var color = await _context.GamePlayers
            .Where(x => x.GameId == gameId && x.UserId == userId)
            .Select(x => (int?)x.Color)
            .FirstOrDefaultAsync(cancellationToken);

        return color.HasValue
            ? (PlayerColor)color.Value
            : null;
    }

    public async Task<PlayerColor> AddOnlinePlayerAsync(
        Guid gameId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var existingColor = await GetPlayerColorAsync(
            gameId,
            userId,
            cancellationToken);

        if (existingColor.HasValue)
            return existingColor.Value;

        var players = await _context.GamePlayers
            .Where(x => x.GameId == gameId && !x.IsBot)
            .ToListAsync(cancellationToken);

        if (players.Count >= 2)
            throw new DomainException("Game already has two players.");

        var color = players.Any(x => x.Color == (int)PlayerColor.Red)
            ? PlayerColor.Green
            : PlayerColor.Red;

        if (players.Any(x => x.Color == (int)color))
            throw new DomainException("Player slot is already taken.");

        _context.GamePlayers.Add(new GamePlayerEntity
        {
            Id = Guid.NewGuid(),
            GameId = gameId,
            UserId = userId,
            Color = (int)color,
            IsBot = false
        });

        await _context.SaveChangesAsync(cancellationToken);

        return color;
    }

    public Task<int> CountHumanPlayersAsync(
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        return _context.GamePlayers.CountAsync(
            x => x.GameId == gameId && !x.IsBot,
            cancellationToken);
    }
}
