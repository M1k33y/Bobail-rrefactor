using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;

namespace Bobail.AI.Analysis.Models;

public sealed record BotProfile(
    string Name,
    BotDifficulty Difficulty,
    EvaluationWeights Weights)
{
    public static IReadOnlyList<BotProfile> CreateDefaults()
    {
        return
        [
            new BotProfile("Easy_Default", BotDifficulty.Easy, new EvaluationWeights()),
            new BotProfile("Medium_Default", BotDifficulty.Medium, new EvaluationWeights()),
            new BotProfile("Hard_Default", BotDifficulty.Hard, new EvaluationWeights())
        ];
    }
}
