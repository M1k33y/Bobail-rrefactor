using Bobail.AI.Analysis.Models;

namespace Bobail.AI.Analysis.Services;

public sealed class AnalysisTableBuilder
{
    public IReadOnlyList<BotRankingRow> BuildRanking(
        IReadOnlyList<BotProfile> profiles,
        IReadOnlyCollection<MatchupSummary> summaries)
    {
        var rows = profiles
            .Select(profile => BuildUnrankedRow(profile.Name, summaries))
            .OrderByDescending(row => row.Score)
            .ThenByDescending(row => row.Winrate)
            .ThenBy(row => row.AverageTurns)
            .ThenBy(row => row.BotName, StringComparer.OrdinalIgnoreCase)
            .Select((row, index) => row with { Rank = index + 1 })
            .ToList();

        return rows;
    }

    public IReadOnlyList<BotProfile> OrderProfilesByRanking(
        IReadOnlyList<BotProfile> profiles,
        IReadOnlyList<BotRankingRow> ranking)
    {
        return ranking
            .Select(row => profiles.Single(profile => NamesEqual(profile.Name, row.BotName)))
            .ToList();
    }

    public static double? GetWinrate(
        string botName,
        string opponentName,
        IReadOnlyCollection<MatchupSummary> summaries)
    {
        var summary = FindSummary(botName, opponentName, summaries);

        if (summary is null)
            return null;

        if (NamesEqual(summary.BotAName, botName))
            return summary.BotAWinrate;

        return summary.BotBWinrate;
    }

    public static double? GetAverageTurns(
        string botName,
        string opponentName,
        IReadOnlyCollection<MatchupSummary> summaries)
    {
        return FindSummary(botName, opponentName, summaries)?.AverageTurns;
    }

    private static BotRankingRow BuildUnrankedRow(
        string botName,
        IReadOnlyCollection<MatchupSummary> summaries)
    {
        int totalGames = 0;
        int wins = 0;
        int losses = 0;
        int draws = 0;
        double weightedTurns = 0;

        foreach (var summary in summaries)
        {
            if (NamesEqual(summary.BotAName, botName))
            {
                totalGames += summary.TotalGames;
                wins += summary.BotAWins;
                losses += summary.BotBWins;
                draws += summary.Draws;
                weightedTurns += summary.AverageTurns * summary.TotalGames;
            }
            else if (NamesEqual(summary.BotBName, botName))
            {
                totalGames += summary.TotalGames;
                wins += summary.BotBWins;
                losses += summary.BotAWins;
                draws += summary.Draws;
                weightedTurns += summary.AverageTurns * summary.TotalGames;
            }
        }

        return new BotRankingRow(
            Rank: 0,
            BotName: botName,
            TotalGames: totalGames,
            Wins: wins,
            Losses: losses,
            Draws: draws,
            Winrate: Percent(wins, totalGames),
            Score: Percent(wins + draws * 0.5, totalGames),
            AverageTurns: totalGames == 0 ? 0 : weightedTurns / totalGames);
    }

    private static MatchupSummary? FindSummary(
        string botName,
        string opponentName,
        IReadOnlyCollection<MatchupSummary> summaries)
    {
        return summaries.FirstOrDefault(summary =>
            (NamesEqual(summary.BotAName, botName) &&
             NamesEqual(summary.BotBName, opponentName)) ||
            (NamesEqual(summary.BotAName, opponentName) &&
             NamesEqual(summary.BotBName, botName)));
    }

    private static double Percent(double numerator, int denominator)
    {
        return denominator == 0
            ? 0
            : numerator / denominator * 100.0;
    }

    private static bool NamesEqual(string left, string right)
    {
        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }
}
