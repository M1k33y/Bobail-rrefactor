using Bobail.AI.Analysis.Models;
using Bobail.AI.Analysis.Services;
using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;

var options = AnalysisOptions.Default;
var outputDirectory = OutputPaths.Create(options.OutputRootDirectory);

var botFactory = new BotFactory(new EvaluationWeights());
var simulationRunner = new SimulationRunner(botFactory, options.MaxTurnsPerGame);
var statisticsAggregator = new StatisticsAggregator();
var csvExporter = new CsvExporter();
var graphGenerator = new GraphGenerator();

var matchupDefinitions = new[]
{
    MatchupDefinition.Create(BotDifficulty.Hard, BotDifficulty.Medium),
    MatchupDefinition.Create(BotDifficulty.Medium, BotDifficulty.Easy),
    MatchupDefinition.Create(BotDifficulty.Hard, BotDifficulty.Easy)
};

var allResults = new List<MatchGameResult>();
var summaries = new List<MatchupSummary>();

Console.WriteLine("Running Bobail difficulty analysis...");
Console.WriteLine($"Games per matchup: {options.GamesPerMatchup}");
Console.WriteLine($"Max turns per game: {options.MaxTurnsPerGame}");
Console.WriteLine();

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

var turnDistributions = statisticsAggregator.BuildWinningTurnDistributions(allResults);

csvExporter.ExportGameResults(Path.Combine(outputDirectory, "game-results.csv"), allResults);
csvExporter.ExportMatchupSummaries(Path.Combine(outputDirectory, "matchup-summary.csv"), summaries);

graphGenerator.SaveWinningTurnDistribution(
    Path.Combine(outputDirectory, "distribution.png"),
    turnDistributions);

graphGenerator.SaveWinrateComparison(
    Path.Combine(outputDirectory, "winrate.png"),
    summaries);

Console.WriteLine($"- {Path.Combine(outputDirectory, "game-results.csv")}");
Console.WriteLine($"- {Path.Combine(outputDirectory, "matchup-summary.csv")}");
Console.WriteLine($"- {Path.Combine(outputDirectory, "distribution.png")}");
Console.WriteLine($"- {Path.Combine(outputDirectory, "winrate.png")}");
