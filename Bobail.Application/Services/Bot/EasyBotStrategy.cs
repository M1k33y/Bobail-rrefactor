using Bobail.Application.Interfaces.Services;
using Bobail.Domain.Board;
using Bobail.Domain.Games;
using Bobail.Domain.Pieces;

namespace Bobail.Application.Services.Bot;

public class EasyBotStrategy : IBotStrategy
{
    private readonly Random _random = new();
    public BotDifficulty Difficulty => BotDifficulty.Easy;

    public BotMove DecideMove(Game game)
    {
        if (game.CurrentPhase == TurnPhase.BobailMoveRequired)
        {
            var moves = game.GetValidBobailMoves();
            var isRed = game.CurrentTurn == PlayerColor.Red;

            List<Position> preferred;

            if (isRed)
            {
                int minRow = moves.Min(m => m.Row);
                preferred = moves.Where(m => m.Row == minRow).ToList();
            }
            else
            {
                int maxRow = moves.Max(m => m.Row);
                preferred = moves.Where(m => m.Row == maxRow).ToList();
            }

            var selected = preferred[_random.Next(preferred.Count)];

            return BotMove.Bobail(selected);
        }

        var pieces = game.Board.Pieces
            .Where(p => !p.IsBobail &&
                        p.Owner == game.CurrentTurn)
            .ToList();

        var possibleMoves =
            new List<(Position from, Position to)>();

        foreach (var piece in pieces)
        {
            var moves = game.GetValidPlayerMoves(piece.Position);

            foreach (var move in moves)
                possibleMoves.Add((piece.Position, move));
        }

        var selectedMove =
            possibleMoves[_random.Next(possibleMoves.Count)];

        return BotMove.Piece(
            selectedMove.from,
            selectedMove.to);
    }
}