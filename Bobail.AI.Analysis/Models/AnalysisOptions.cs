namespace Bobail.AI.Analysis.Models;

public sealed record AnalysisOptions(
    int GamesPerMatchup,
    int MaxTurnsPerGame,
    string OutputRootDirectory,
    bool IncludeDefaultProfiles,
    string? ProfileInputDirectory,
    IReadOnlyList<string> ProfilePaths)
{
    public static AnalysisOptions Default => new(
        GamesPerMatchup: 300,
        MaxTurnsPerGame: 200,
        OutputRootDirectory: Path.Combine(ProjectDirectory, "analysis-output"),
        IncludeDefaultProfiles: true,
        ProfileInputDirectory: Environment.GetEnvironmentVariable("BOBAIL_BOT_PROFILE_DIR")
            ?? Path.Combine(ProjectDirectory, "bot-profiles"),
        ProfilePaths: []);

    private static string ProjectDirectory => Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
}
