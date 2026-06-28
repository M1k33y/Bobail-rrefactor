using Bobail.Domain.Games;
using Bobail.Infrastructure.Bots;

namespace Bobail.Application.Services.Bot;

public sealed class GameRepetitionTracker
{
    public const int DefaultOccurrencesRequired = 3;

    private readonly int _occurrencesRequired;
    private readonly Dictionary<SearchBoardStateKey, int> _occurrences = new();

    public GameRepetitionTracker(int occurrencesRequired = DefaultOccurrencesRequired)
    {
        if (occurrencesRequired < 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(occurrencesRequired),
                "At least two occurrences are required to detect repetition.");
        }

        _occurrencesRequired = occurrencesRequired;
    }

    public bool Record(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        var state = SearchBoardStateKeyBuilder.FromGame(game);
        int count = _occurrences.GetValueOrDefault(state) + 1;
        _occurrences[state] = count;

        return count >= _occurrencesRequired;
    }
}
