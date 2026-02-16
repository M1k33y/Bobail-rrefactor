using Bobail.Application.Interfaces.Repositories;
using Bobail.Domain.Board;
using Bobail.Domain.Common;
using Bobail.Domain.Games;

namespace Bobail.Application.Services;

public class GameService
{
    private readonly IGameRepository _repository;

    public GameService(IGameRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> CreateGameAsync(CancellationToken cancellationToken = default)
    {
        var game = new Game();

        await _repository.AddAsync(game, cancellationToken);

        return game.Id;
    }

    public async Task<Game> GetGameAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var game = await _repository.GetByIdAsync(gameId, cancellationToken);

        if (game is null)
            throw new DomainException("Game not found.");

        return game;
    }

    public async Task ExecuteBobailMoveAsync(
        Guid gameId,
        int toRow,
        int toColumn,
        CancellationToken cancellationToken = default)
    {
        var game = await GetGameAsync(gameId, cancellationToken);

        var target = new Position(toRow, toColumn);

        game.ExecuteBobailMove(target);

        await _repository.UpdateAsync(game, cancellationToken);
    }

    public async Task ExecutePlayerMoveAsync(
        Guid gameId,
        int fromRow,
        int fromColumn,
        int toRow,
        int toColumn,
        CancellationToken cancellationToken = default)
    {
        var game = await GetGameAsync(gameId, cancellationToken);

        var from = new Position(fromRow, fromColumn);
        var to = new Position(toRow, toColumn);

        game.ExecutePlayerMove(from, to);

        await _repository.UpdateAsync(game, cancellationToken);
    }

    public async Task<List<(int row, int col)>> GetValidPlayerMovesAsync(
    Guid gameId,
    int row,
    int col)
    {
        var game = await GetGameAsync(gameId);

        var moves = game.GetValidPlayerMoves(new Position(row, col));

        return moves.Select(m => (m.Row, m.Column)).ToList();
    }
}
