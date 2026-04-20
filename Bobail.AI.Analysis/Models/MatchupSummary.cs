namespace Bobail.AI.Analysis.Models;

public sealed record MatchupSummary(
    string MatchupName,
    string BotAName,
    string BotBName,
    string ExpectedStrongerName,
    int TotalGames,
    int BotAWins,
    int BotBWins,
    int Draws,
    double BotAWinrate,
    double BotBWinrate,
    double AverageTurns,
    int MinTurns,
    int MaxTurns,
    double ExpectedStrongerWinrate,
    double OneSidedPValue,
    bool StatisticallySignificant);
