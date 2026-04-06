using Bobail.Application.Interfaces.Repositories;
using Bobail.Infrastructure.Persistence;

public class SqlGamePlayerRepository : IGamePlayerRepository
{
    private readonly GameDbContext _context;

    public SqlGamePlayerRepository(GameDbContext context)
    {
        _context = context;
    }

    public async Task AddPlayersForGame(Guid gameId, Guid userId, bool isVsBot)
    {
        var players = new List<GamePlayerEntity>();

        if (isVsBot)
        {
            players.Add(new GamePlayerEntity
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                UserId = userId,
                Color = 0,
                IsBot = false
            });

            players.Add(new GamePlayerEntity
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                UserId = null,
                Color = 1,
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
                Color = 0,
                IsBot = false
            });

            //  (future multiplayer)
        }

        _context.GamePlayers.AddRange(players);
        await _context.SaveChangesAsync();
    }
}