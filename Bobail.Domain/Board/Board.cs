using Bobail.Domain.Games;
using Bobail.Domain.Pieces;

namespace Bobail.Domain.Board;

public class Board
{
    private readonly List<Piece> _pieces;

    public IReadOnlyCollection<Piece> Pieces => _pieces.AsReadOnly();

    // constructor normal
    public Board()
    {
        _pieces = new List<Piece>();
        Initialize();
    }

    // constructor pt clone
    private Board(List<Piece> pieces)
    {
        _pieces = pieces;
    }

    private void Initialize()
    {
        // Red row (row 0)
        for (int col = 0; col < 5; col++)
            _pieces.Add(new Piece(
                PieceType.PlayerPiece,
                new Position(0, col),
                PlayerColor.Red));

        // Green row (row 4)
        for (int col = 0; col < 5; col++)
            _pieces.Add(new Piece(
                PieceType.PlayerPiece,
                new Position(4, col),
                PlayerColor.Green));

        // Bobail center
        _pieces.Add(new Piece(
            PieceType.Bobail,
            new Position(2, 2)));
    }

    public Piece? GetPieceAt(Position position)
        => _pieces.FirstOrDefault(p => p.Position.Equals(position));

    public bool IsEmpty(Position position)
        => GetPieceAt(position) == null;

    public void MovePiece(Piece piece, Position target)
    {
        piece.MoveTo(target);
    }

    public Board Clone()
    {
        var clonedPieces = _pieces
            .Select(p => p.Clone())
            .ToList();

        return new Board(clonedPieces);
    }
}