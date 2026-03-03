using Bobail.Domain.Board;
using Bobail.Domain.Games;
using Bobail.Domain.Pieces;
using Bobail.Application.Interfaces.Services;

namespace Bobail.Application.Services.Bot;

public class EasyBotService : IBotService
{
    private readonly Random _random = new();

    public Task ExecuteSingleMoveAsync(Game game)
    {
        if (!game.IsBotTurn())
            return Task.CompletedTask;

        
        if (game.CurrentPhase == TurnPhase.BobailMoveRequired)
        {
            ExecuteBobailMove(game);
            return Task.CompletedTask;
        }

       
        if (game.CurrentPhase == TurnPhase.PlayerMoveRequired)
        {
            ExecutePieceMove(game);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    private void ExecuteBobailMove(Game game)
    {
        var bobailMoves = game.GetValidBobailMoves();

        if (!bobailMoves.Any())
            return;

        var isRed = game.CurrentTurn == PlayerColor.Red;

        List<Position> preferredMoves;

        if (isRed)
        {
            int minRow = bobailMoves.Min(m => m.Row);
            preferredMoves = bobailMoves
                .Where(m => m.Row == minRow)
                .ToList();
        }
        else
        {
            int maxRow = bobailMoves.Max(m => m.Row);
            preferredMoves = bobailMoves
                .Where(m => m.Row == maxRow)
                .ToList();
        }

        var selectedMove =
            preferredMoves[_random.Next(preferredMoves.Count)];

        game.ExecuteBobailMove(selectedMove);
    }

    private void ExecutePieceMove(Game game)
    {
        var botPieces = game.Board.Pieces
            .Where(p => !p.IsBobail &&
                        p.Owner == game.CurrentTurn)
            .ToList();

        var possibleMoves =
            new List<(Position from, Position to)>();

        foreach (var piece in botPieces)
        {
            var moves =
                game.GetValidPlayerMoves(piece.Position);

            foreach (var move in moves)
            {
                possibleMoves.Add((piece.Position, move));
            }
        }

        if (!possibleMoves.Any())
            return;

        var selectedMove =
            possibleMoves[_random.Next(possibleMoves.Count)];

        game.ExecutePlayerMove(
            selectedMove.from,
            selectedMove.to);
    }
}