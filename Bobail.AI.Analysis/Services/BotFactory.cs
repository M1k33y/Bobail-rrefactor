using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;
using Bobail.Infrastructure.Bots;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bobail.AI.Analysis.Services;

public sealed class BotFactory
{
    private readonly EvaluationWeights _hardWeights;

    public BotFactory(EvaluationWeights hardWeights)
    {
        _hardWeights = hardWeights;
    }

    public IBotStrategy Create(BotDifficulty difficulty)
    {
        return difficulty switch
        {
            BotDifficulty.Easy => new EasyBotStrategy(),
            BotDifficulty.Medium => new MediumBotStrategy(
                new MediumBoardEvaluator(_hardWeights),
                NullLogger<MediumBotStrategy>.Instance),
            BotDifficulty.Hard => new HardBotStrategy(
                new HardBoardEvaluator(_hardWeights),
                NullLogger<HardBotStrategy>.Instance),
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, "Unsupported difficulty.")
        };
    }
}
