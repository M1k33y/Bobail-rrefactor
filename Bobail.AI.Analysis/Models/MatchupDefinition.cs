namespace Bobail.AI.Analysis.Models;

public sealed record MatchupDefinition(
    BotProfile BotA,
    BotProfile BotB)
{
    public string BotAName => BotA.Name;

    public string BotBName => BotB.Name;

    public static IReadOnlyList<MatchupDefinition> CreateRoundRobin(IReadOnlyList<BotProfile> profiles)
    {
        var matchups = new List<MatchupDefinition>();

        for (int i = 0; i < profiles.Count; i++)
        {
            for (int j = i + 1; j < profiles.Count; j++)
            {
                matchups.Add(new MatchupDefinition(profiles[i], profiles[j]));
            }
        }

        return matchups;
    }
}
