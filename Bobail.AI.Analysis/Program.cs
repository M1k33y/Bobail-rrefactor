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

csvExporter.ExportGameResults(Path.Combine(outputDirectory, "game-results.csv"), allResults);
csvExporter.ExportMatchupSummaries(Path.Combine(outputDirectory, "matchup-summary.csv"), summaries);
csvExporter.ExportWinrateMatrix(Path.Combine(outputDirectory, "winrate-matrix.csv"), profiles, summaries);

graphGenerator.SaveWinrateComparison(
    Path.Combine(outputDirectory, "winrate.png"),
    summaries);

Console.WriteLine($"- {Path.Combine(outputDirectory, "game-results.csv")}");
Console.WriteLine($"- {Path.Combine(outputDirectory, "matchup-summary.csv")}");
Console.WriteLine($"- {Path.Combine(outputDirectory, "winrate-matrix.csv")}");
Console.WriteLine($"- {Path.Combine(outputDirectory, "winrate.png")}");
