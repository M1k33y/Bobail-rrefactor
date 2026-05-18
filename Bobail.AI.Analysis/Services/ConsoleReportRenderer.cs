using Bobail.AI.Analysis.Models;

namespace Bobail.AI.Analysis.Services;

public static class ConsoleReportRenderer
{
    public static void WriteSummary(MatchupSummary summary)
    {
        Console.WriteLine($"Matchup: {summary.MatchupName}");
        Console.WriteLine($"  Games: {summary.TotalGames}");
        Console.WriteLine($"  {summary.BotAName} wins: {summary.BotAWins} ({summary.BotAWinrate:F2}%)");
        Console.WriteLine($"  {summary.BotBName} wins: {summary.BotBWins} ({summary.BotBWinrate:F2}%)");
        Console.WriteLine($"  Draws / turn limit: {summary.Draws}");
        Console.WriteLine($"  Leader: {summary.LeaderName} ({summary.LeaderWinrate:F2}% of decisive games)");
        Console.WriteLine($"  Starter wins: {summary.BotAName}={summary.BotAStartedWins}/{summary.BotAStartedGames}, {summary.BotBName}={summary.BotBStartedWins}/{summary.BotBStartedGames}");
        Console.WriteLine($"  p-value: {summary.OneSidedLeaderPValue:F6}, significant={summary.StatisticallySignificant}");
        Console.WriteLine($"  Turns: avg={summary.AverageTurns:F2}, min={summary.MinTurns}, max={summary.MaxTurns}");
    }
}
