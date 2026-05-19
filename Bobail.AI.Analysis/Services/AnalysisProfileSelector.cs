using Bobail.AI.Analysis.Models;
using Bobail.Domain.Games;

namespace Bobail.AI.Analysis.Services;

public sealed class AnalysisProfileSelector
{
    public IReadOnlyList<BotProfile> SelectProfiles(
        AnalysisOptions options,
        BotProfileLoader profileLoader)
    {
        return options.Workflow switch
        {
            AnalysisWorkflow.DefaultProfiles => BotProfile.CreateDefaults(),
            AnalysisWorkflow.HardProfiles => SelectHardProfiles(options, profileLoader),
            AnalysisWorkflow.AllProfiles => profileLoader.Load(options),
            _ => throw new ArgumentOutOfRangeException(
                nameof(options),
                options.Workflow,
                "Unsupported analysis workflow.")
        };
    }

    private static IReadOnlyList<BotProfile> SelectHardProfiles(
        AnalysisOptions options,
        BotProfileLoader profileLoader)
    {
        var profiles = profileLoader
            .Load(options with { IncludeDefaultProfiles = true })
            .Where(profile => profile.Difficulty == BotDifficulty.Hard)
            .ToList();

        if (profiles.Count < 2)
            throw new InvalidOperationException(
                "HardProfiles workflow requires at least two hard profiles.");

        return profiles;
    }
}
