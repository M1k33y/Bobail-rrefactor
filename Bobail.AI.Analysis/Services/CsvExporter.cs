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
            "MatchupName,BotAName,BotBName,LeaderName,TotalGames,BotAWins,BotBWins,Draws,BotAWinrate,BotBWinrate,LeaderWinrate,BotAStartedGames,BotAStartedWins,BotBStartedGames,BotBStartedWins,BotAStartedWinrate,BotBStartedWinrate,AverageTurns,MinTurns,MaxTurns,OneSidedLeaderPValue,StatisticallySignificant"
        };

        lines.AddRange(summaries.Select(summary =>
            string.Join(",",
                Escape(summary.MatchupName),
                Escape(summary.BotAName),
                Escape(summary.BotBName),
                Escape(summary.LeaderName),
                summary.TotalGames.ToString(CultureInfo.InvariantCulture),
                summary.BotAWins.ToString(CultureInfo.InvariantCulture),
                summary.BotBWins.ToString(CultureInfo.InvariantCulture),
                summary.Draws.ToString(CultureInfo.InvariantCulture),
                summary.BotAWinrate.ToString("F4", CultureInfo.InvariantCulture),
                summary.BotBWinrate.ToString("F4", CultureInfo.InvariantCulture),
                summary.LeaderWinrate.ToString("F4", CultureInfo.InvariantCulture),
                summary.BotAStartedGames.ToString(CultureInfo.InvariantCulture),
                summary.BotAStartedWins.ToString(CultureInfo.InvariantCulture),
                summary.BotBStartedGames.ToString(CultureInfo.InvariantCulture),
                summary.BotBStartedWins.ToString(CultureInfo.InvariantCulture),
                summary.BotAStartedWinrate.ToString("F4", CultureInfo.InvariantCulture),
                summary.BotBStartedWinrate.ToString("F4", CultureInfo.InvariantCulture),
                summary.AverageTurns.ToString("F4", CultureInfo.InvariantCulture),
                summary.MinTurns.ToString(CultureInfo.InvariantCulture),
                summary.MaxTurns.ToString(CultureInfo.InvariantCulture),
                summary.OneSidedLeaderPValue.ToString("F6", CultureInfo.InvariantCulture),
                summary.StatisticallySignificant.ToString(CultureInfo.InvariantCulture))));

        File.WriteAllLines(outputPath, lines, Encoding.UTF8);
    }

    public void ExportWinrateMatrix(
        string outputPath,
        IReadOnlyList<BotProfile> profiles,
        IReadOnlyCollection<MatchupSummary> summaries)
    {
        var lines = new List<string>
        {
            "Bot \\ Opponent (win-rate %)," + string.Join(",", profiles.Select(profile => Escape(profile.Name)))
        };

        foreach (var rowProfile in profiles)
        {
            var values = new List<string> { Escape(rowProfile.Name) };

            foreach (var columnProfile in profiles)
            {
                if (rowProfile.Name == columnProfile.Name)
                {
                    values.Add("self");
                    continue;
                }

                var winrate = AnalysisTableBuilder.GetWinrate(rowProfile.Name, columnProfile.Name, summaries);
                values.Add(winrate is null
                    ? string.Empty
                    : winrate.Value.ToString("F2", CultureInfo.InvariantCulture));
            }

            lines.Add(string.Join(",", values));
        }

        File.WriteAllLines(outputPath, lines, Encoding.UTF8);
    }

    public void ExportAverageTurnsMatrix(
        string outputPath,
        IReadOnlyList<BotProfile> profiles,
        IReadOnlyCollection<MatchupSummary> summaries)
    {
        var lines = new List<string>
        {
            "Bot \\ Opponent (average turns)," + string.Join(",", profiles.Select(profile => Escape(profile.Name)))
        };

        foreach (var rowProfile in profiles)
        {
            var values = new List<string> { Escape(rowProfile.Name) };

            foreach (var columnProfile in profiles)
            {
                if (rowProfile.Name == columnProfile.Name)
                {
                    values.Add("self");
                    continue;
                }

                var averageTurns = AnalysisTableBuilder.GetAverageTurns(rowProfile.Name, columnProfile.Name, summaries);
                values.Add(averageTurns is null
                    ? string.Empty
                    : averageTurns.Value.ToString("F2", CultureInfo.InvariantCulture));
            }

            lines.Add(string.Join(",", values));
        }

        File.WriteAllLines(outputPath, lines, Encoding.UTF8);
    }

    public void ExportOverallRanking(string outputPath, IReadOnlyCollection<BotRankingRow> ranking)
    {
        var lines = new List<string>
        {
            "Rank,BotName,Score,Winrate,Wins,Losses,Draws,TotalGames,AverageTurns"
        };

        lines.AddRange(ranking.Select(row =>
            string.Join(",",
                row.Rank.ToString(CultureInfo.InvariantCulture),
                Escape(row.BotName),
                row.Score.ToString("F2", CultureInfo.InvariantCulture),
                row.Winrate.ToString("F2", CultureInfo.InvariantCulture),
                row.Wins.ToString(CultureInfo.InvariantCulture),
                row.Losses.ToString(CultureInfo.InvariantCulture),
                row.Draws.ToString(CultureInfo.InvariantCulture),
                row.TotalGames.ToString(CultureInfo.InvariantCulture),
                row.AverageTurns.ToString("F2", CultureInfo.InvariantCulture))));

        File.WriteAllLines(outputPath, lines, Encoding.UTF8);
    }

    private static string Escape(string value)
    {
        if (!value.Contains(',') && !value.Contains('"'))
            return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

}
