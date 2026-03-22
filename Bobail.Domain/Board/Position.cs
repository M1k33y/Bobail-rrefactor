using Bobail.Domain.Board;
using Bobail.Domain.Common;
using System.Text.Json.Serialization;

public sealed class Position : ValueObject
{
    public int Row { get; private set; }
    public int Column { get; private set; }

    [JsonConstructor]
    public Position(int row, int column)
    {
        if (row < 0 || row > 4)
            throw new DomainException("Row must be between 0 and 4.");

        if (column < 0 || column > 4)
            throw new DomainException("Column must be between 0 and 4.");

        Row = row;
        Column = column;
    }

    private Position() { } 
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