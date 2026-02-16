using Bobail.Domain.Common;
using Bobail.Domain.Board;

namespace Bobail.Domain.Pieces;

public class Piece : Entity
{
    public PieceType Type { get; }
    public Position Position { get; private set; }

    public bool IsBobail => Type == PieceType.Bobail;

    public Piece(PieceType type, Position position)
    {
        Type = type;
        Position = position;
    }

    public void MoveTo(Position newPosition)
    {
        Position = newPosition;
    }
}
