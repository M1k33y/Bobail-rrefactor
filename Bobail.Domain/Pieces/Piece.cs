using Bobail.Domain.Board;
using Bobail.Domain.Common;
using Bobail.Domain.Games;

namespace Bobail.Domain.Pieces;

public class Piece : Entity
{
    public PieceType Type { get; private set; }
    public PlayerColor? Owner { get; private set; }
    public Position Position { get; private set; }

    public bool IsBobail => Type == PieceType.Bobail;

    public Piece(PieceType type, Position position, PlayerColor? owner = null)
    {
        Id = Guid.NewGuid();

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

    public Piece Clone()
    {
        return new Piece(
            Type,
            new Position(Position.Row, Position.Column),
            Owner);
    }
}
