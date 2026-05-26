using Bobail.AI.Analysis.Models;
using Bobail.AI.Analysis.Services;

var options = AnalysisOptions.Default;
var outputDirectory = OutputPaths.Create(options.OutputRootDirectory);

var profileLoader = new BotProfileLoader();
var profileSelector = new AnalysisProfileSelector();
var profiles = profileSelector.SelectProfiles(options, profileLoader);
var botFactory = new BotFactory();
var simulationRunner = new SimulationRunner(botFactory, options.MaxTurnsPerGame);
var statisticsAggregator = new StatisticsAggregator();
var tableBuilder = new AnalysisTableBuilder();
var csvExporter = new CsvExporter();
var graphGenerator = new GraphGenerator();
var matchupDefinitions = MatchupDefinition.CreateRoundRobin(profiles);

var allResults = new List<MatchGameResult>();
var summaries = new List<MatchupSummary>();

Console.WriteLine("Running Bobail bot profile analysis...");
Console.WriteLine($"Workflow: {options.Workflow}");
Console.WriteLine($"Games per matchup: {options.GamesPerMatchup}");
Console.WriteLine($"Max turns per game: {options.MaxTurnsPerGame}");
Console.WriteLine($"Profile directory: {options.ProfileInputDirectory ?? "(none)"}");
Console.WriteLine("Profiles:");
foreach (var profile in profiles)
{
    Console.WriteLine($"- {profile.Name} ({profile.Difficulty})");
}

Console.WriteLine();

if (profiles.Count < 2)
{
    Console.WriteLine("At least two bot profiles are required for analysis.");
    return;
}

foreach (var matchup in matchupDefinitions)
{
    Console.WriteLine($"Simulating {matchup.BotAName} vs {matchup.BotBName}...");

    var results = simulationRunner.RunMatchup(matchup, options.GamesPerMatchup);
    var summary = statisticsAggregator.BuildSummary(matchup, results);

    allResults.AddRange(results);
    summaries.Add(summary);

    ConsoleReportRenderer.WriteSummary(summary);
    Console.WriteLine();
}

var ranking = tableBuilder.BuildRanking(profiles, summaries);
var rankedProfiles = tableBuilder.OrderProfilesByRanking(profiles, ranking);

csvExporter.ExportGameResults(Path.Combine(outputDirectory, "game-results.csv"), allResults);
csvExporter.ExportMatchupSummaries(Path.Combine(outputDirectory, "matchup-summary.csv"), summaries);
csvExporter.ExportWinrateMatrix(Path.Combine(outputDirectory, "winrate-matrix.csv"), rankedProfiles, summaries);
csvExporter.ExportAverageTurnsMatrix(Path.Combine(outputDirectory, "average-turns-matrix.csv"), rankedProfiles, summaries);
csvExporter.ExportOverallRanking(Path.Combine(outputDirectory, "overall-ranking.csv"), ranking);

graphGenerator.SavePairwiseWinrateHeatmap(
    Path.Combine(outputDirectory, "pairwise-winrate-heatmap.png"),
    rankedProfiles,
    summaries);
graphGenerator.SaveOverallRanking(
    Path.Combine(outputDirectory, "overall-ranking.png"),
    ranking);
graphGenerator.SaveAverageTurnsHeatmap(
    Path.Combine(outputDirectory, "average-turns-heatmap.png"),
    rankedProfiles,
    summaries);

Console.WriteLine($"- {Path.Combine(outputDirectory, "game-results.csv")}");
Console.WriteLine($"- {Path.Combine(outputDirectory, "matchup-summary.csv")}");
Console.WriteLine($"- {Path.Combine(outputDirectory, "overall-ranking.csv")}");
Console.WriteLine($"- {Path.Combine(outputDirectory, "winrate-matrix.csv")}");
Console.WriteLine($"- {Path.Combine(outputDirectory, "average-turns-matrix.csv")}");
Console.WriteLine($"- {Path.Combine(outputDirectory, "pairwise-winrate-heatmap.png")}");
Console.WriteLine($"- {Path.Combine(outputDirectory, "overall-ranking.png")}");
Console.WriteLine($"- {Path.Combine(outputDirectory, "average-turns-heatmap.png")}");
