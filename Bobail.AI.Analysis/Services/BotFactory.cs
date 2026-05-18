using Bobail.AI.Analysis.Models;
using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;
using Bobail.Infrastructure.Bots;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bobail.AI.Analysis.Services;

public sealed class BotFactory
{
    public IBotStrategy Create(BotProfile profile)
    {
        return profile.Difficulty switch
        {
            BotDifficulty.Easy => new EasyBotStrategy(),
            BotDifficulty.Medium => new MediumBotStrategy(
                new MediumBoardEvaluator(profile.Weights),
                NullLogger<MediumBotStrategy>.Instance),
            BotDifficulty.Hard => new HardBotStrategy(
                new HardBoardEvaluator(profile.Weights),
                NullLogger<HardBotStrategy>.Instance),
            _ => throw new ArgumentOutOfRangeException(
                nameof(profile),
                profile.Difficulty,
                "Unsupported difficulty.")
        };
    }
}
