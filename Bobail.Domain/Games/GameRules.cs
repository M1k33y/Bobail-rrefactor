
using Bobail.Domain.Board;
using Bobail.Domain.Pieces;
using Bobail.Domain.Common;


namespace Bobail.Domain.Games;
public static class GameRules
{
    public static void ValidateBobailMove(Game game, Position target)
    {
        if (game.Status != GameStatus.InProgress)
            throw new DomainException("Game is not active.");

        var bobail = game.Board.Pieces.FirstOrDefault(p => p.IsBobail);

        if (bobail is null)
            throw new DomainException("Bobail not found on board.");

        var from = bobail.Position;

        if (from.Equals(target))
            throw new DomainException("Bobail must move to a different position.");

        if (!game.Board.IsEmpty(target))
            throw new DomainException("Target position is not empty.");

        int deltaRow = Math.Abs(target.Row - from.Row);
        int deltaCol = Math.Abs(target.Column - from.Column);

        // Must move exactly one square
        if (deltaRow > 1 || deltaCol > 1)
            throw new DomainException("Bobail can only move one square.");

        // Must move at least one axis
        if (deltaRow == 0 && deltaCol == 0)
            throw new DomainException("Invalid Bobail movement.");
    }

    public static void ValidatePlayerMove(Game game, Position from, Position to)
    {
        if (game.Status != GameStatus.InProgress)
            throw new DomainException("Game is not active.");

        var piece = game.Board.GetPieceAt(from);

        if (piece is null)
            throw new DomainException("No piece at source position.");

        if (piece.IsBobail)
            throw new DomainException("Cannot move Bobail as a player token.");

        if (!BelongsToCurrentPlayer(piece, game.CurrentTurn))
            throw new DomainException("Cannot move opponent's piece.");

        if (from.Equals(to))
            throw new DomainException("Source and destination cannot be the same.");

        if (!game.Board.IsEmpty(to))
            throw new DomainException("Target position is not empty.");

        var direction = Direction.FromPositions(from, to);

        if (!direction.IsStraightOrDiagonal())
            throw new DomainException("Invalid movement direction.");

        var farthestPosition = GetFarthestPosition(game, from, direction);

        if (!to.Equals(farthestPosition))
            throw new DomainException("Player token must move as far as possible in the chosen direction.");
    }


    private static bool BelongsToCurrentPlayer(Piece piece, PlayerColor currentTurn)
    {
        return piece.Owner == currentTurn;
    }

    private static void ValidatePathClear(Game game, Position from, Position to, Direction direction)
    {
        var current = from.Move(direction);

        while (!current.Equals(to))
        {
            if (!game.Board.IsEmpty(current))
                throw new DomainException("Cannot jump over other pieces.");

            current = current.Move(direction);
        }
    }

    public static void ApplyBobailMove(Game game, Position target)
    {
        var bobail = game.Board.Pieces.First(p => p.IsBobail);

        game.Board.MovePiece(bobail, target);
    }

    public static void ApplyPlayerMove(Game game, Position from, Position to)
    {
        var piece = game.Board.GetPieceAt(from)
            ?? throw new DomainException("Piece not found.");

        game.Board.MovePiece(piece, to);
    }

    private static bool IsBobailSurrounded(Game game)
    {
        var bobail = game.Board.Pieces.First(p => p.IsBobail);
        var position = bobail.Position;

        var directions = new List<Direction>
    {
        new Direction(-1, 0),  // up
        new Direction(1, 0),   // down
        new Direction(0, -1),  // left
        new Direction(0, 1),   // right
        new Direction(-1, -1), // up-left
        new Direction(-1, 1),  // up-right
        new Direction(1, -1),  // down-left
        new Direction(1, 1)    // down-right
    };

        foreach (var direction in directions)
        {
            var newRow = position.Row + direction.DeltaRow;
            var newCol = position.Column + direction.DeltaColumn;

            if (newRow < 0 || newRow > 4 || newCol < 0 || newCol > 4)
                continue;

            var neighbor = new Position(newRow, newCol);

            if (game.Board.IsEmpty(neighbor))
                return false; // exista mutare posibila
        }

        return true; // niciun pătrat liber
    }

    public static void CheckVictory(Game game)
    {
        var bobail = game.Board.Pieces.First(p => p.IsBobail);
        var position = bobail.Position;

        //Bobail reaches home row
        if (position.Row == 0)
        {
            game.Finish(PlayerColor.Red);
            return;
        }

        if (position.Row == 4)
        {
            game.Finish(PlayerColor.Green);
            return;
        }

        // Bobail surrounded
        if (IsBobailSurrounded(game))
        {
            game.Finish(game.CurrentTurn);
        }
    }

    private static Position GetFarthestPosition(Game game, Position from, Direction direction)
    {
        var currentRow = from.Row;
        var currentCol = from.Column;

        while (true)
        {
            var nextRow = currentRow + direction.DeltaRow;
            var nextCol = currentCol + direction.DeltaColumn;

            
            if (nextRow < 0 || nextRow > 4 || nextCol < 0 || nextCol > 4)
                break;

            var nextPosition = new Position(nextRow, nextCol);

            if (!game.Board.IsEmpty(nextPosition))
                break;

            currentRow = nextRow;
            currentCol = nextCol;
        }

        return new Position(currentRow, currentCol);
    }

    public static List<Position> GetValidPlayerMoves(Game game, Position from)
    {
        var piece = game.Board.GetPieceAt(from);

        if (piece is null)
            return new List<Position>();

        if (piece.IsBobail)
            return new List<Position>();

        if (!BelongsToCurrentPlayer(piece, game.CurrentTurn))
            return new List<Position>();

        var directions = new List<Direction>
    {
        new Direction(-1, 0),
        new Direction(1, 0),
        new Direction(0, -1),
        new Direction(0, 1),
        new Direction(-1, -1),
        new Direction(-1, 1),
        new Direction(1, -1),
        new Direction(1, 1)
    };

        var validMoves = new List<Position>();

        foreach (var direction in directions)
        {
            var farthest = GetFarthestPosition(game, from, direction);

            if (!farthest.Equals(from))
                validMoves.Add(farthest);
        }

        return validMoves;
    }

    public static List<Position> GetValidBobailMoves(Game game)
    {
        var bobail = game.Board.Pieces.First(p => p.IsBobail);
        var from = bobail.Position;

        var directions = new List<Direction>
    {
        new Direction(-1, 0),
        new Direction(1, 0),
        new Direction(0, -1),
        new Direction(0, 1),
        new Direction(-1, -1),
        new Direction(-1, 1),
        new Direction(1, -1),
        new Direction(1, 1)
    };

        var validMoves = new List<Position>();

        foreach (var direction in directions)
        {
            var newRow = from.Row + direction.DeltaRow;
            var newCol = from.Column + direction.DeltaColumn;

            if (newRow < 0 || newRow > 4 || newCol < 0 || newCol > 4)
                continue;

            var target = new Position(newRow, newCol);

            if (game.Board.IsEmpty(target))
                validMoves.Add(target);
        }

        return validMoves;
    }

}
