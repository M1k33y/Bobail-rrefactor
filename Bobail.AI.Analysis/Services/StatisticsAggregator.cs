using Bobail.AI.Analysis.Models;

namespace Bobail.AI.Analysis.Services;

public sealed class StatisticsAggregator
{
    public MatchupSummary BuildSummary(MatchupDefinition matchup, IReadOnlyList<MatchGameResult> results)
    {
        int botAWins = results.Count(result => result.Winner == matchup.BotAName);
        int botBWins = results.Count(result => result.Winner == matchup.BotBName);
        int draws = results.Count - botAWins - botBWins;

        double totalGames = results.Count;
        int decisiveGames = botAWins + botBWins;
        var leaderName = ResolveLeaderName(matchup, botAWins, botBWins);
        double leaderWins = leaderName == matchup.BotAName ? botAWins : botBWins;
        double leaderWinrate = decisiveGames == 0 || leaderName == "Tie"
            ? 0
            : leaderWins / decisiveGames * 100.0;
        double pValue = decisiveGames == 0 || leaderName == "Tie"
            ? 1.0
            : CalculateOneSidedPValue(leaderWins, decisiveGames);

        var botAStartedGames = results.Count(result => result.StartingBotName == matchup.BotAName);
        var botBStartedGames = results.Count(result => result.StartingBotName == matchup.BotBName);
        var botAStartedWins = results.Count(result =>
            result.StartingBotName == matchup.BotAName &&
            result.Winner == matchup.BotAName);
        var botBStartedWins = results.Count(result =>
            result.StartingBotName == matchup.BotBName &&
            result.Winner == matchup.BotBName);

        return new MatchupSummary(
            MatchupName: $"{matchup.BotAName} vs {matchup.BotBName}",
            BotAName: matchup.BotAName,
            BotBName: matchup.BotBName,
            LeaderName: leaderName,
            TotalGames: results.Count,
            BotAWins: botAWins,
            BotBWins: botBWins,
            Draws: draws,
            BotAWinrate: botAWins / totalGames * 100.0,
            BotBWinrate: botBWins / totalGames * 100.0,
            LeaderWinrate: leaderWinrate,
            BotAStartedGames: botAStartedGames,
            BotAStartedWins: botAStartedWins,
            BotBStartedGames: botBStartedGames,
            BotBStartedWins: botBStartedWins,
            BotAStartedWinrate: Percent(botAStartedWins, botAStartedGames),
            BotBStartedWinrate: Percent(botBStartedWins, botBStartedGames),
            AverageTurns: results.Average(result => result.Turns),
            MinTurns: results.Min(result => result.Turns),
            MaxTurns: results.Max(result => result.Turns),
            OneSidedLeaderPValue: pValue,
            StatisticallySignificant: pValue < 0.05);
    }

    private static double CalculateOneSidedPValue(double observedWins, int trials)
    {
        double expectedWins = trials * 0.5;
        double standardDeviation = Math.Sqrt(trials * 0.5 * 0.5);

        if (standardDeviation == 0)
            return 1.0;

        double zScore = (observedWins - expectedWins) / standardDeviation;
        return 0.5 * Erfc(zScore / Math.Sqrt(2));
    }

    private static string ResolveLeaderName(MatchupDefinition matchup, int botAWins, int botBWins)
    {
        if (botAWins == botBWins)
            return "Tie";

        return botAWins > botBWins
            ? matchup.BotAName
            : matchup.BotBName;
    }

    private static double Percent(int numerator, int denominator)
    {
        return denominator == 0
            ? 0
            : numerator / (double)denominator * 100.0;
    }

    private static double Erfc(double x)
    {
        return 1 - Erf(x);
    }

    private static double Erf(double x)
    {
        double sign = Math.Sign(x);
        x = Math.Abs(x);

        const double a1 = 0.254829592;
        const double a2 = -0.284496736;
        const double a3 = 1.421413741;
        const double a4 = -1.453152027;
        const double a5 = 1.061405429;
        const double p = 0.3275911;

        double t = 1.0 / (1.0 + p * x);
        double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

        return sign * y;
    }
}
