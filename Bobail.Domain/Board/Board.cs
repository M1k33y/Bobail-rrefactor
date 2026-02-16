using Bobail.Domain.Pieces;

namespace Bobail.Domain.Board;

public class Board
{
    private readonly List<Piece> _pieces = new();

    public IReadOnlyCollection<Piece> Pieces => _pieces.AsReadOnly();

    public Board()
    {
        Initialize();
    }

    private void Initialize()
    {
        // Red row (row 0)
        for (int col = 0; col < 5; col++)
            _pieces.Add(new Piece(PieceType.Red, new Position(0, col)));

        // Green row (row 4)
        for (int col = 0; col < 5; col++)
            _pieces.Add(new Piece(PieceType.Green, new Position(4, col)));

        // Bobail center
        _pieces.Add(new Piece(PieceType.Bobail, new Position(2, 2)));
    }

    public Piece? GetPieceAt(Position position)
        => _pieces.FirstOrDefault(p => p.Position.Equals(position));

    public bool IsEmpty(Position position)
        => GetPieceAt(position) == null;

    public void MovePiece(Piece piece, Position target)
    {
        piece.MoveTo(target);
    }
}
