using Bobail.Domain.Board;

namespace Bobail.Application.Services.Bot;

public class BotMove
{
    public bool IsBobailMove { get; }
    public Position From { get; }
    public Position To { get; }

    private BotMove(bool isBobailMove, Position from, Position to)
    {
        IsBobailMove = isBobailMove;
        From = from;
        To = to;
    }

    public static BotMove Bobail(Position to)
        => new BotMove(true, null!, to);

    public static BotMove Piece(Position from, Position to)
        => new BotMove(false, from, to);
}