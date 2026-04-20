namespace Bobail.AI.Analysis.Models;

public sealed record AnalysisOptions(int GamesPerMatchup, int MaxTurnsPerGame, string OutputRootDirectory)
{
    public static AnalysisOptions Default => new(
        GamesPerMatchup: 600,
        MaxTurnsPerGame: 80,
        OutputRootDirectory: Path.Combine(AppContext.BaseDirectory, "analysis-output"));
}
