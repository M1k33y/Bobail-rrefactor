using Bobail.Application.Services.Bot;

namespace Bobail.AI.Analysis.Models;

public sealed class BotProfileFile
{
    public string Name { get; init; } = string.Empty;

    public string Difficulty { get; init; } = "Hard";

    public EvaluationWeights Weights { get; init; } = new();
}
