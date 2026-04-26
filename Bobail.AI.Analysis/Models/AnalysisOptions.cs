namespace Bobail.AI.Analysis.Models;

public sealed record AnalysisOptions(int GamesPerMatchup, int MaxTurnsPerGame, string OutputRootDirectory)
{
    public static AnalysisOptions Default => new(
        GamesPerMatchup: 300,
        MaxTurnsPerGame: 100,
        OutputRootDirectory: Path.Combine(AppContext.BaseDirectory, "analysis-output"));
}
