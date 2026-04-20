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
        Console.WriteLine($"  Turns: avg={summary.AverageTurns:F2}, min={summary.MinTurns}, max={summary.MaxTurns}");
        Console.WriteLine(
            $"  Expected stronger ({summary.ExpectedStrongerName}) winrate: {summary.ExpectedStrongerWinrate:F2}% | " +
            $"p={summary.OneSidedPValue:F4} | significant={summary.StatisticallySignificant}");
    }
}
