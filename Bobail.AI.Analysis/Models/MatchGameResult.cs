namespace Bobail.AI.Analysis.Models;

public sealed record MatchGameResult(
    string BotAName,
    string BotBName,
    string StartingBotName,
    string? Winner,
    int Turns,
    bool ReachedTurnLimit);
