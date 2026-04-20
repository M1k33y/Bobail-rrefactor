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

    public void SaveWinningTurnDistribution(string outputPath, IReadOnlyDictionary<string, List<int>> turnDistributions)
    {
        var plot = new Plot();

        foreach (var pair in turnDistributions.OrderBy(x => BotSortKey(x.Key)))
        {
            if (pair.Value.Count == 0)
                continue;

            double[] turns = pair.Value.Select(static x => (double)x).ToArray();
            int minTurn = pair.Value.Min();
            int maxTurn = pair.Value.Max();
            int binCount = Math.Max(6, Math.Min(20, maxTurn - minTurn + 1));

            var histogram = ScottPlot.Statistics.Histogram.WithBinCount(binCount, turns);
            var scatter = plot.Add.Scatter(histogram.Bins, histogram.Counts);
            scatter.LegendText = $"{pair.Key} wins";
            scatter.Color = ResolveColor(pair.Key);
            scatter.LineWidth = 2;
        }

        plot.Title("Bobail Winning Turn Distribution");
        plot.XLabel("Turns");
        plot.YLabel("Frequency");
        plot.ShowLegend();
        plot.SavePng(outputPath, 1200, 800);
    }

    public void SaveWinrateComparison(string outputPath, IReadOnlyList<MatchupSummary> summaries)
    {
        var plot = new Plot();
        var bars = summaries
            .Select((summary, index) => new Bar
            {
                Position = index,
                Value = summary.ExpectedStrongerWinrate,
                Label = summary.MatchupName,
                FillColor = ResolveColor(summary.ExpectedStrongerName),
                ValueLabel = $"{summary.ExpectedStrongerWinrate:F1}%"
            })
            .ToArray();

        plot.Add.Bars(bars);
        plot.Axes.Bottom.SetTicks(
            summaries.Select((_, index) => (double)index).ToArray(),
            summaries.Select(summary => summary.MatchupName).ToArray());

        plot.Title("Expected Stronger Bot Winrate");
        plot.YLabel("Winrate (%)");
        plot.SavePng(outputPath, 1200, 800);
    }

    private static Color ResolveColor(string botName)
    {
        return BotColors.TryGetValue(botName, out var color)
            ? color
            : Colors.SlateGray;
    }

    private static int BotSortKey(string botName)
    {
        return botName switch
        {
            "Easy" => 0,
            "Medium" => 1,
            "Hard" => 2,
            _ => int.MaxValue
        };
    }
}
