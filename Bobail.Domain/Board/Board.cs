using Bobail.Domain.Games;
using Bobail.Domain.Pieces;
using System.Text.Json.Serialization;

namespace Bobail.Domain.Board;

public class Board
{
    
    public List<Piece> Pieces { get; set; }

    [JsonConstructor]
    public Board(List<Piece> pieces)
    {
        Pieces = pieces;
    }

    public Board()
    {
        Pieces = new List<Piece>();
        Initialize();
    }

    private void Initialize()
    {
        for (int col = 0; col < 5; col++)
            Pieces.Add(new Piece(
                PieceType.PlayerPiece,
                new Position(0, col),
                PlayerColor.Red));

        for (int col = 0; col < 5; col++)
            Pieces.Add(new Piece(
                PieceType.PlayerPiece,
                new Position(4, col),
                PlayerColor.Green));

        Pieces.Add(new Piece(
            PieceType.Bobail,
            new Position(2, 2)));
    }

    public Piece? GetPieceAt(Position position)
    {
        Console.WriteLine($"SEARCHING: {position.Row},{position.Column}");

        foreach (var p in Pieces)
        {
            Console.WriteLine($"PIECE: {p.Position.Row},{p.Position.Column}");
        }

        return Pieces.FirstOrDefault(p =>
            p.Position.Row == position.Row &&
            p.Position.Column == position.Column);
    }

    public bool IsEmpty(Position position)
        => GetPieceAt(position) == null;

    public void MovePiece(Piece piece, Position target)
    {
        piece.MoveTo(target);
    }

    public Board Clone()
    {
        var clonedPieces = Pieces
            .Select(p => p.Clone())
            .ToList();

        return new Board(clonedPieces);
    }
}