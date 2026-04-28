using System.Globalization;
using System.Text;
using Bobail.AI.Analysis.Models;

namespace Bobail.AI.Analysis.Services;

public sealed class CsvExporter
{
    public void ExportGameResults(string outputPath, IReadOnlyCollection<MatchGameResult> results)
    {
        var lines = new List<string>
        {
            "BotAName,BotBName,StartingBotName,Winner,Turns,ReachedTurnLimit"
        };

        lines.AddRange(results.Select(result =>
            string.Join(",",
                Escape(result.BotAName),
                Escape(result.BotBName),
                Escape(result.StartingBotName),
                Escape(result.Winner ?? "None"),
                result.Turns.ToString(CultureInfo.InvariantCulture),
                result.ReachedTurnLimit.ToString(CultureInfo.InvariantCulture))));

        File.WriteAllLines(outputPath, lines, Encoding.UTF8);
    }

    public void ExportMatchupSummaries(string outputPath, IReadOnlyCollection<MatchupSummary> summaries)
    {
        var lines = new List<string>
        {
            "MatchupName,BotAName,BotBName,ExpectedStrongerName,TotalGames,BotAWins,BotBWins,Draws,BotAWinrate,BotBWinrate,AverageTurns,MinTurns,MaxTurns,ExpectedStrongerWinrate,OneSidedPValue,StatisticallySignificant"
        };

        lines.AddRange(summaries.Select(summary =>
            string.Join(",",
                Escape(summary.MatchupName),
                Escape(summary.BotAName),
                Escape(summary.BotBName),
                Escape(summary.ExpectedStrongerName),
                summary.TotalGames.ToString(CultureInfo.InvariantCulture),
                summary.BotAWins.ToString(CultureInfo.InvariantCulture),
                summary.BotBWins.ToString(CultureInfo.InvariantCulture),
                summary.Draws.ToString(CultureInfo.InvariantCulture),
                summary.BotAWinrate.ToString("F4", CultureInfo.InvariantCulture),
                summary.BotBWinrate.ToString("F4", CultureInfo.InvariantCulture),
                summary.AverageTurns.ToString("F4", CultureInfo.InvariantCulture),
                summary.MinTurns.ToString(CultureInfo.InvariantCulture),
                summary.MaxTurns.ToString(CultureInfo.InvariantCulture),
                summary.ExpectedStrongerWinrate.ToString("F4", CultureInfo.InvariantCulture),
                summary.OneSidedPValue.ToString("F6", CultureInfo.InvariantCulture),
                summary.StatisticallySignificant.ToString(CultureInfo.InvariantCulture))));

        File.WriteAllLines(outputPath, lines, Encoding.UTF8);
    }

    private static string Escape(string value)
    {
        if (!value.Contains(',') && !value.Contains('"'))
            return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
