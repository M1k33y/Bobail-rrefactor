using Bobail.AI.Analysis.Models;
using ScottPlot;

namespace Bobail.AI.Analysis.Services;

public sealed class GraphGenerator
{
    private static readonly Dictionary<string, Color> BotColors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Easy"] = Colors.Orange,
        ["Medium"] = Colors.SeaGreen,
        ["Hard"] = Colors.Crimson
    };

    public void SaveWinrateComparison(string outputPath, IReadOnlyList<MatchupSummary> summaries)
    {
        var plot = new Plot();
        var bars = summaries
            .Select((summary, index) => new Bar
            {
                Position = index,
                Value = summary.LeaderWinrate,
                Label = summary.MatchupName,
                FillColor = ResolveColor(summary.LeaderName),
                ValueLabel = $"{summary.LeaderWinrate:F1}%"
            })
            .ToArray();

        plot.Add.Bars(bars);
        plot.Axes.Bottom.SetTicks(
            summaries.Select((_, index) => (double)index).ToArray(),
            summaries.Select(summary => summary.MatchupName).ToArray());

        plot.Title("Matchup Leader Winrate");
        plot.YLabel("Winrate (%)");
        plot.SavePng(outputPath, 1200, 800);
    }

    private static Color ResolveColor(string botName)
    {
        if (botName.Contains("Easy", StringComparison.OrdinalIgnoreCase))
            return Colors.Orange;

        if (botName.Contains("Medium", StringComparison.OrdinalIgnoreCase))
            return Colors.SeaGreen;

        if (botName.Contains("Hard", StringComparison.OrdinalIgnoreCase) ||
            botName.Contains("GA", StringComparison.OrdinalIgnoreCase))
        {
            return Colors.Crimson;
        }

        return BotColors.TryGetValue(botName, out var color)
            ? color
            : Colors.SlateGray;
    }
}
