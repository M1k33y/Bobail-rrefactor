using Bobail.Application.DTOs;
using Bobail.Application.Interfaces.Repositories;
using Bobail.Infrastructure.Persistence;
using Bobail.Infrastructure.Persistance.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bobail.Infrastructure.Persistance.Repositories;

public class SqlGameHistoryRepository : IGameHistoryRepository
{
    private readonly GameDbContext _context;
    private readonly IGameStateRepository _gameStateRepository;

    public SqlGameHistoryRepository(
        GameDbContext context,
        IGameStateRepository gameStateRepository)
    {
        _context = context;
        _gameStateRepository = gameStateRepository;
    }

    public async Task<PagedGameHistoryResponse> GetHistoryForUserAsync(
        Guid userId,
        GameHistoryQuery query,
        CancellationToken cancellationToken = default)
    {
        var playerEntries = await _context.GamePlayers
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        var gameIds = playerEntries
            .Select(x => x.GameId)
            .Distinct()
            .ToList();

        if (gameIds.Count == 0)
        {
            return new PagedGameHistoryResponse
            {
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = 0,
                TotalPages = 0,
                HasPreviousPage = query.Page > 1,
                HasNextPage = false,
                Items = new List<GameHistoryResponse>()
            };
        }

        var gamesQuery = _context.Games
            .Where(x => gameIds.Contains(x.Id) && x.Status == (int)Domain.Games.GameStatus.Finished)
            .OrderByDescending(x => x.UpdatedAt);

        var totalCount = await gamesQuery.CountAsync(cancellationToken);
        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)query.PageSize);
        var currentPage = totalPages == 0
            ? 1
            : Math.Min(query.Page, totalPages);
        var skip = (currentPage - 1) * query.PageSize;

        var games = await gamesQuery
            .Skip(skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var participants = await _context.GamePlayers
            .Where(x => games.Select(g => g.Id).Contains(x.GameId))
            .ToListAsync(cancellationToken);

        var userIds = participants
            .Where(x => x.UserId.HasValue)
            .Select(x => x.UserId!.Value)
            .Distinct()
            .ToList();

        var users = await _context.Users
            .Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Nickname, cancellationToken);

        var items = games.Select(game =>
        {
            var gameParticipants = participants
                .Where(x => x.GameId == game.Id)
                .ToList();

            var opponent = gameParticipants
                .FirstOrDefault(x => x.IsBot)
                ?? gameParticipants.FirstOrDefault(x => x.UserId != userId);

            var opponentName = ResolveOpponentName(opponent, users);
            var botDifficulty = ResolveBotDifficulty(game.Mode, game.BotDifficulty);
            var playedVs = BuildPlayedVsLabel(opponentName, botDifficulty);

            return new GameHistoryResponse
            {
                GameId = game.Id,
                OpponentName = opponentName,
                PlayedVs = playedVs,
                Result = game.WinnerUserId == userId ? "Win" : "Loss",
                PlayedAtUtc = game.UpdatedAt,
                Mode = ((Domain.Games.GameMode)game.Mode).ToString(),
                BotDifficulty = botDifficulty
            };
        }).ToList();

        return new PagedGameHistoryResponse
        {
            Page = currentPage,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = currentPage > 1,
            HasNextPage = currentPage < totalPages,
            Items = items
        };
    }

    public async Task<UserGameStatsResponse?> GetUserStatsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
            return null;

        var participantEntries = await _context.GamePlayers
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        if (participantEntries.Count == 0)
        {
            return new UserGameStatsResponse
            {
                MemberSince = user.CreatedAt
            };
        }

        var participantByGameId = participantEntries
            .GroupBy(x => x.GameId)
            .ToDictionary(x => x.Key, x => x.First());

        var gameIds = participantByGameId.Keys.ToList();

        var finishedGames = await _context.Games
            .AsNoTracking()
            .Where(x =>
                gameIds.Contains(x.Id) &&
                x.Status == (int)Domain.Games.GameStatus.Finished)
            .ToListAsync(cancellationToken);

        return new UserGameStatsResponse
        {
            MemberSince = user.CreatedAt,
            TotalGamesPlayed = finishedGames.Count,
            TotalWins = finishedGames.Count(x => x.WinnerUserId == userId),
            TotalLosses = finishedGames.Count(x => x.WinnerUserId != userId),
            WinsWithGreen = CountByColorAndResult(finishedGames, participantByGameId, userId, Domain.Games.PlayerColor.Green, true),
            WinsWithRed = CountByColorAndResult(finishedGames, participantByGameId, userId, Domain.Games.PlayerColor.Red, true),
            LossesWithGreen = CountByColorAndResult(finishedGames, participantByGameId, userId, Domain.Games.PlayerColor.Green, false),
            LossesWithRed = CountByColorAndResult(finishedGames, participantByGameId, userId, Domain.Games.PlayerColor.Red, false)
        };
    }

    public async Task<GameReplayResponse?> GetReplayAsync(
        Guid gameId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userIsParticipant = await _context.GamePlayers
            .AnyAsync(x => x.GameId == gameId && x.UserId == userId, cancellationToken);

        if (!userIsParticipant)
            return null;

        var game = await _context.Games
            .FirstOrDefaultAsync(x => x.Id == gameId && x.Status == (int)Domain.Games.GameStatus.Finished, cancellationToken);

        if (game == null)
            return null;

        var participants = await _context.GamePlayers
            .Where(x => x.GameId == gameId)
            .ToListAsync(cancellationToken);

        var opponent = participants
            .FirstOrDefault(x => x.IsBot)
            ?? participants.FirstOrDefault(x => x.UserId != userId);

        var userMap = await _context.Users
            .ToDictionaryAsync(x => x.Id, x => x.Nickname, cancellationToken);

        var opponentName = ResolveOpponentName(opponent, userMap);
        var botDifficulty = ResolveBotDifficulty(game.Mode, game.BotDifficulty);
        var playedVs = BuildPlayedVsLabel(opponentName, botDifficulty);

        var states = await _gameStateRepository.GetByGameIdAsync(gameId, cancellationToken);

        return new GameReplayResponse
        {
            GameId = gameId,
            OpponentName = opponentName,
            PlayedVs = playedVs,
            Result = game.WinnerUserId == userId ? "Win" : "Loss",
            PlayedAtUtc = game.UpdatedAt,
            BotDifficulty = botDifficulty,
            States = states.Select(x =>
            {
                var state = GameSerializer.Deserialize(x.StateJson);

                return new GameReplayStateResponse
                {
                    MoveNumber = x.MoveNumber,
                    CreatedAtUtc = x.CreatedAtUtc,
                    Status = state.Status.ToString(),
                    CurrentTurn = state.CurrentTurn.ToString(),
                    Winner = state.Winner?.ToString(),
                    IsFirstTurn = state.IsFirstTurn,
                    CurrentPhase = state.CurrentPhase.ToString(),
                    Mode = state.Mode.ToString(),
                    BotColor = state.BotColor?.ToString(),
                    Pieces = state.Board.Pieces.Select(p => new PieceDto
                    {
                        Type = p.Type.ToString(),
                        Owner = p.Owner?.ToString(),
                        Row = p.Position.Row,
                        Column = p.Position.Column
                    }).ToList()
                };
            }).ToList()
        };
    }

    private static string ResolveOpponentName(GamePlayerEntity? opponent, IReadOnlyDictionary<Guid, string> users)
    {
        if (opponent == null)
            return "Player2";

        if (opponent.IsBot)
            return "BOT";

        if (opponent.UserId.HasValue && users.TryGetValue(opponent.UserId.Value, out var nickname))
            return nickname;

        return "Player2";
    }

    private static string? ResolveBotDifficulty(int mode, int? botDifficulty)
    {
        if (mode != (int)Domain.Games.GameMode.PlayerVsBot || !botDifficulty.HasValue)
            return null;

        return ((Domain.Games.BotDifficulty)botDifficulty.Value).ToString();
    }

    private static string BuildPlayedVsLabel(string opponentName, string? botDifficulty)
    {
        if (opponentName == "BOT" && !string.IsNullOrWhiteSpace(botDifficulty))
            return $"BOT {botDifficulty}";

        return opponentName;
    }

    private static int CountByColorAndResult(
        IEnumerable<GameEntity> games,
        IReadOnlyDictionary<Guid, GamePlayerEntity> participantByGameId,
        Guid userId,
        Domain.Games.PlayerColor color,
        bool isWin)
    {
        return games.Count(game =>
        {
            if (!participantByGameId.TryGetValue(game.Id, out var participant))
                return false;

            var hasColor = participant.Color == (int)color;
            var hasResult = isWin
                ? game.WinnerUserId == userId
                : game.WinnerUserId != userId;

            return hasColor && hasResult;
        });
    }
}
