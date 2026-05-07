using Bobail.Application.Interfaces.Services;
using Bobail.Domain.Games;
using Bobail.Infrastructure.Bots;

namespace Bobail.Application.Services.Bot;

public class MediumBoardEvaluator : IBoardEvaluator
{
    private readonly HardBoardEvaluator _hardEvaluator;

    public MediumBoardEvaluator(EvaluationWeights baseWeights)
    {
        _hardEvaluator = new HardBoardEvaluator(baseWeights);
    }

    public int Evaluate(Game game, PlayerColor botColor)
    {
        return _hardEvaluator.Evaluate(game, botColor);
    }
}
