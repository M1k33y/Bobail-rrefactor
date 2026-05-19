using Bobail.Domain.Common;
using FluentAssertions;

namespace Bobail.Domain.Tests.Board;

public class PositionTests
{
    [Theory]
    [InlineData(-1, 0, "Row must be between 0 and 4.")]
    [InlineData(5, 0, "Row must be between 0 and 4.")]
    [InlineData(0, -1, "Column must be between 0 and 4.")]
    [InlineData(0, 5, "Column must be between 0 and 4.")]
    public void Position_Rejects_Coordinates_Outside_Board(int row, int column, string message)
    {
        var act = () => new Position(row, column);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(message);
    }
}
