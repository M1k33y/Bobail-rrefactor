using Bobail.Domain.Common;

namespace Bobail.Domain.Board;

public sealed class Direction : ValueObject
{
    public int DeltaRow { get; }
    public int DeltaColumn { get; }

    public Direction(int deltaRow, int deltaColumn)
    {
        DeltaRow = deltaRow;
        DeltaColumn = deltaColumn;
    }

    public static Direction FromPositions(Position from, Position to)
    {
        var deltaRow = to.Row - from.Row;
        var deltaCol = to.Column - from.Column;

        if (deltaRow == 0 && deltaCol == 0)
            throw new DomainException("Source and destination cannot be the same.");

        int normalizedRow = Normalize(deltaRow);
        int normalizedCol = Normalize(deltaCol);

        return new Direction(normalizedRow, normalizedCol);
    }

    public bool IsStraightOrDiagonal()
    {
        return
            DeltaRow == 0 ||
            DeltaColumn == 0 ||
            Math.Abs(DeltaRow) == Math.Abs(DeltaColumn);
    }

    private static int Normalize(int value)
    {
        if (value == 0) return 0;
        return value > 0 ? 1 : -1;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DeltaRow;
        yield return DeltaColumn;
    }
}
