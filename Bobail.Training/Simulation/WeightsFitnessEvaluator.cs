using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;
using Bobail.Infrastructure.Bots;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bobail.Training.Simulation;

public sealed class WeightsFitnessEvaluator
{
    
    private readonly GameSimulator _simulator = new();
    private readonly TrainingSettings _settings;

    public WeightsFitnessEvaluator(TrainingSettings settings)
    {
        _settings = settings;
    }

    public double EvaluateFitness(EvaluationWeights weights)
    {
        double totalFitness = 0;

        for (int gameIndex = 0; gameIndex < _settings.GamesPerGenome; gameIndex++)
        {
            // Alternate the candidate's color so the first-move advantage stays balanced
            bool candidateStarts = gameIndex % 2 == 0;
            var candidateBot = CreateHardBot(weights);
            var baselineBot = new EasyBotStrategy();

            var result = candidateStarts
                ? _simulator.PlayGame(candidateBot, baselineBot, _settings.MaxTurnsPerGame)
                : _simulator.PlayGame(baselineBot, candidateBot, _settings.MaxTurnsPerGame);

            var candidateColor = candidateStarts ? PlayerColor.Red : PlayerColor.Green;
            totalFitness += ScoreResult(result, candidateColor);
        }

        return totalFitness;
    }

    private static HardBotStrategy CreateHardBot(EvaluationWeights weights)
    {
        var evaluator = new HardBoardEvaluator(weights);
        return new HardBotStrategy(evaluator, NullLogger<HardBotStrategy>.Instance);
    }

    private static double ScoreResult(GameResult result, PlayerColor candidateColor)
    {
        int turns = result.Turns;

        // f2(x) = 1/x
        if (result.Winner == candidateColor)
            return 1000.0 / turns;

        return -1000.0 / turns;
    }
}
