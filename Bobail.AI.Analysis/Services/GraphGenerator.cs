using Bobail.AI.Analysis.Models;
using ScottPlot;

namespace Bobail.AI.Analysis.Services;

public sealed class GraphGenerator
{
    private const int HeatmapCellLabelFontSize = 28;
    private const int HeatmapTickLabelFontSize = 20;
    private const int HeatmapAxisLabelFontSize = 24;
    private const int HeatmapTitleFontSize = 26;
    private const int HeatmapColorBarLabelFontSize = 22;
    private const int HeatmapColorBarTickFontSize = 18;

    public void SavePairwiseWinrateHeatmap(
        string outputPath,
        IReadOnlyList<BotProfile> profiles,
        IReadOnlyCollection<MatchupSummary> summaries)
    {
        var values = BuildMatrix(
            profiles,
            (rowBot, columnBot) => rowBot.Name == columnBot.Name
                ? double.NaN
                : AnalysisTableBuilder.GetWinrate(rowBot.Name, columnBot.Name, summaries));

        var plot = CreateHeatmapPlot(
            title: "Pairwise Win-rate Heatmap",
            xLabel: "Opponent",
            yLabel: "Bot",
            profiles: profiles,
            values: values,
            valueLabel: value => double.IsNaN(value) ? "self" : $"{value:F1}%",
            textColor: value => double.IsNaN(value) || value < 62 ? Colors.Black : Colors.White);

        var heatmap = plot.GetPlottables().OfType<ScottPlot.Plottables.Heatmap>().Single();
        heatmap.Colormap = new ScottPlot.Colormaps.CustomInterpolated(
        [
            Colors.Crimson,
            Colors.White,
            Colors.SeaGreen
        ]);
        heatmap.ManualRange = new ScottPlot.Range(0, 100);
        heatmap.NaNCellColor = Colors.LightGray;

        var colorBar = plot.Add.ColorBar(heatmap);
        colorBar.Label = "Win-rate (%)";
        StyleHeatmapColorBar(colorBar);
        plot.SavePng(outputPath, 1200, 850);
    }

    public void SaveOverallRanking(
        string outputPath,
        IReadOnlyList<BotRankingRow> ranking)
    {
        var plot = new Plot();
        var bars = ranking
            .Select((row, index) => new Bar
            {
                Position = index,
                Value = row.Score,
                FillColor = index == 0 ? Colors.SeaGreen : Colors.SlateGray,
                ValueLabel = $"{row.Score:F1}%"
            })
            .ToArray();

        plot.Add.Bars(bars);
        plot.Title("Overall Bot Ranking by Score");
        plot.YLabel("Score (%)");
        plot.Axes.Bottom.SetTicks(
            ranking.Select((_, index) => (double)index).ToArray(),
            ranking.Select(row => row.BotName).ToArray());
        plot.Axes.SetLimits(-0.5, ranking.Count - 0.5, 0, 100);
        plot.SavePng(outputPath, 1200, 750);
    }

    public void SaveAverageTurnsHeatmap(
        string outputPath,
        IReadOnlyList<BotProfile> profiles,
        IReadOnlyCollection<MatchupSummary> summaries)
    {
        var values = BuildMatrix(
            profiles,
            (rowBot, columnBot) => rowBot.Name == columnBot.Name
                ? double.NaN
                : AnalysisTableBuilder.GetAverageTurns(rowBot.Name, columnBot.Name, summaries));

        var plot = CreateHeatmapPlot(
            title: "Average Game Length per Matchup",
            xLabel: "Opponent",
            yLabel: "Bot",
            profiles: profiles,
            values: values,
            valueLabel: value => double.IsNaN(value) ? "self" : $"{value:F1}",
            textColor: _ => Colors.Black);

        var heatmap = plot.GetPlottables().OfType<ScottPlot.Plottables.Heatmap>().Single();
        heatmap.Colormap = new ScottPlot.Colormaps.Viridis();
        heatmap.NaNCellColor = Colors.LightGray;

        var colorBar = plot.Add.ColorBar(heatmap);
        colorBar.Label = "Average turns";
        StyleHeatmapColorBar(colorBar);
        plot.SavePng(outputPath, 1200, 850);
    }

    private static Plot CreateHeatmapPlot(
        string title,
        string xLabel,
        string yLabel,
        IReadOnlyList<BotProfile> profiles,
        double[,] values,
        Func<double, string> valueLabel,
        Func<double, Color> textColor)
    {
        var plot = new Plot();
        plot.Add.Heatmap(values);

        plot.Title(title, HeatmapTitleFontSize);
        plot.XLabel(xLabel, HeatmapAxisLabelFontSize);
        plot.YLabel(yLabel, HeatmapAxisLabelFontSize);
        plot.Axes.Bottom.SetTicks(
            profiles.Select((_, index) => (double)index).ToArray(),
            profiles.Select(profile => profile.Name).ToArray());
        plot.Axes.Left.SetTicks(
            profiles.Select((_, index) => (double)index).ToArray(),
            profiles.Reverse().Select(profile => profile.Name).ToArray());
        plot.Axes.Bottom.TickLabelStyle.FontSize = HeatmapTickLabelFontSize;
        plot.Axes.Left.TickLabelStyle.FontSize = HeatmapTickLabelFontSize;
        plot.Axes.Margins(0, 0);

        for (int rowIndex = 0; rowIndex < profiles.Count; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < profiles.Count; columnIndex++)
            {
                var value = values[rowIndex, columnIndex];
                var yPosition = profiles.Count - 1 - rowIndex;
                var text = plot.Add.Text(valueLabel(value), columnIndex, yPosition);
                text.Alignment = Alignment.MiddleCenter;
                text.LabelFontSize = HeatmapCellLabelFontSize;
                text.LabelFontColor = textColor(value);
            }
        }

        return plot;
    }

    private static void StyleHeatmapColorBar(ScottPlot.Panels.ColorBar colorBar)
    {
        colorBar.LabelStyle.FontSize = HeatmapColorBarLabelFontSize;
        colorBar.Axis.TickLabelStyle.FontSize = HeatmapColorBarTickFontSize;
    }

    private static double[,] BuildMatrix(
        IReadOnlyList<BotProfile> profiles,
        Func<BotProfile, BotProfile, double?> valueFactory)
    {
        var values = new double[profiles.Count, profiles.Count];

        for (int rowIndex = 0; rowIndex < profiles.Count; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < profiles.Count; columnIndex++)
            {
                values[rowIndex, columnIndex] = valueFactory(profiles[rowIndex], profiles[columnIndex])
                    ?? double.NaN;
            }
        }

        return values;
    }
}
