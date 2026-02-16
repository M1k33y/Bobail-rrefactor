using Bobail.Domain.Common;

namespace Bobail.Domain.Board;

public sealed class Position : ValueObject
{
    public int Row { get; }
    public int Column { get; }

    public Position(int row, int column)
    {
        if (row < 0 || row > 4)
            throw new DomainException("Row must be between 0 and 4.");

        if (column < 0 || column > 4)
            throw new DomainException("Column must be between 0 and 4.");

        Row = row;
        Column = column;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Row;
        yield return Column;
    }

    public Position Move(Direction direction)
    {
        return new Position(Row + direction.DeltaRow,
                            Column + direction.DeltaColumn);
    }
}
