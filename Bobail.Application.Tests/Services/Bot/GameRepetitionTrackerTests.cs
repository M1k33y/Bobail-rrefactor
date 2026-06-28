using Bobail.Application.Services.Bot;
using Bobail.Domain.Board;
using Bobail.Domain.Games;
using FluentAssertions;

namespace Bobail.Application.Tests.Services.Bot;

public class GameRepetitionTrackerTests
{
    [Fact]
    public void Record_ReturnsTrue_WhenSameStateOccursThreeTimes()
    {
        var game = new Game();
        var tracker = new GameRepetitionTracker();

        tracker.Record(game).Should().BeFalse();
        tracker.Record(game.Clone()).Should().BeFalse();
        tracker.Record(game.Clone()).Should().BeTrue();
    }

    [Fact]
    public void Record_CountsDifferentStatesSeparately()
    {
        var game = new Game();
        var initialState = game.Clone();
        var tracker = new GameRepetitionTracker();

        tracker.Record(initialState).Should().BeFalse();
        tracker.Record(initialState.Clone()).Should().BeFalse();

        game.ExecutePlayerMove(new Position(0, 0), new Position(3, 0));

        tracker.Record(game).Should().BeFalse();
        tracker.Record(game.Clone()).Should().BeFalse();
        tracker.Record(game.Clone()).Should().BeTrue();
    }

    [Fact]
    public void Constructor_RejectsOccurrenceThresholdBelowTwo()
    {
        var act = () => new GameRepetitionTracker(1);

        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("occurrencesRequired");
    }
}
