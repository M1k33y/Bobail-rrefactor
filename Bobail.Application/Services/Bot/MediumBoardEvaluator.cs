using Bobail.Application.Interfaces.Services;
using Bobail.Domain.Games;
using Bobail.Infrastructure.Bots;

namespace Bobail.Application.Services.Bot;

public class MediumBoardEvaluator : IBoardEvaluator
{
    private readonly HardBoardEvaluator _hardEvaluator;

    public MediumBoardEvaluator(EvaluationWeights baseWeights)
    {
        _hardEvaluator = new HardBoardEvaluator(CreateMediumWeights(baseWeights));
    }

    public int Evaluate(Game game, PlayerColor botColor)
    {
        return _hardEvaluator.Evaluate(game, botColor);
    }

    private static EvaluationWeights CreateMediumWeights(EvaluationWeights baseWeights)
    {
        return new EvaluationWeights
        {
            ProgressWeight = Scale(baseWeights.ProgressWeight, 0.85),
            PathToGoalWeight = Scale(baseWeights.PathToGoalWeight, 0.70),
            ImmediateWinThreatWeight = Scale(baseWeights.ImmediateWinThreatWeight, 0.65),
            ImmediateLossThreatWeight = Scale(baseWeights.ImmediateLossThreatWeight, 0.55),
            BobailMobilityWeight = Scale(baseWeights.BobailMobilityWeight, 0.80),
            ForwardMobilityWeight = Scale(baseWeights.ForwardMobilityWeight, 0.75),
            TrapRiskWeight = Scale(baseWeights.TrapRiskWeight, 0.60),
            OpponentPressureWeight = Scale(baseWeights.OpponentPressureWeight, 0.65),
            FriendlySupportWeight = Scale(baseWeights.FriendlySupportWeight, 0.75),
            DestinationQualityWeight = Scale(baseWeights.DestinationQualityWeight, 0.60)
        };
    }

    private static int Scale(int value, double factor)
    {
        return Math.Max(1, (int)Math.Round(value * factor));
    }
}
