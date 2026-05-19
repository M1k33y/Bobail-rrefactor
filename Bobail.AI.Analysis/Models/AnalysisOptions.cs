namespace Bobail.AI.Analysis.Models;

public sealed record AnalysisOptions(
    AnalysisWorkflow Workflow,
    int GamesPerMatchup,
    int MaxTurnsPerGame,
    string OutputRootDirectory,
    bool IncludeDefaultProfiles,
    string? ProfileInputDirectory,
    IReadOnlyList<string> ProfilePaths)
{
    public static AnalysisOptions Default => new(
        Workflow: ReadWorkflow(),
        GamesPerMatchup: 300,
        MaxTurnsPerGame: 200,
        OutputRootDirectory: Path.Combine(ProjectDirectory, "analysis-output"),
        IncludeDefaultProfiles: true,
        ProfileInputDirectory: Environment.GetEnvironmentVariable("BOBAIL_BOT_PROFILE_DIR")
            ?? Path.Combine(ProjectDirectory, "bot-profiles"),
        ProfilePaths: []);

    private static string ProjectDirectory => Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

    private static AnalysisWorkflow ReadWorkflow()
    {
        var value = Environment.GetEnvironmentVariable("BOBAIL_ANALYSIS_WORKFLOW");

        if (string.IsNullOrWhiteSpace(value))
            return AnalysisWorkflow.HardProfiles; //switch pentru defaults sau hards

        return Enum.TryParse<AnalysisWorkflow>(value, ignoreCase: true, out var workflow)
            ? workflow
            : throw new InvalidOperationException(
                $"Invalid BOBAIL_ANALYSIS_WORKFLOW '{value}'. Use DefaultProfiles, HardProfiles, or AllProfiles.");
    }
}
