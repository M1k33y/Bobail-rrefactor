using Bobail.Domain.Games;

namespace Bobail.AI.Analysis.Models;

public sealed record MatchupDefinition(
    BotDifficulty BotA,
    BotDifficulty BotB,
    BotDifficulty ExpectedStronger)
{
    public string BotAName => BotA.ToString();

    public string BotBName => BotB.ToString();

    public string ExpectedStrongerName => ExpectedStronger.ToString();

    public static MatchupDefinition Create(BotDifficulty botA, BotDifficulty botB)
    {
        var expectedStronger = (BotDifficulty)Math.Max((int)botA, (int)botB);
        return new MatchupDefinition(botA, botB, expectedStronger);
    }
}
