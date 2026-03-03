using Bobail.Domain.Common;
using Bobail.Domain.Board;
using Bobail.Domain.Games;

namespace Bobail.Domain.Pieces;

public class Piece : Entity
{
    public PieceType Type { get; }
    public PlayerColor? Owner { get; }
    public Position Position { get; private set; }

    public bool IsBobail => Type == PieceType.Bobail;

    public Piece(PieceType type, Position position, PlayerColor? owner = null)
    {
        if (type == PieceType.PlayerPiece && owner is null)
            throw new DomainException("PlayerPiece must have an owner.");

        if (type == PieceType.Bobail && owner is not null)
            throw new DomainException("Bobail cannot have an owner.");

        Type = type;
        Position = position;
        Owner = owner;
    }

    public void MoveTo(Position newPosition)
    {
        Position = newPosition;
    }
}