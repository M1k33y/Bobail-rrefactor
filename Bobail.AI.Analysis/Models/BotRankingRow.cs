namespace Bobail.AI.Analysis.Models;

public sealed record BotRankingRow(
    int Rank,
    string BotName,
    int TotalGames,
    int Wins,
    int Losses,
    int Draws,
    double Winrate,
    double Score,
    double AverageTurns);
