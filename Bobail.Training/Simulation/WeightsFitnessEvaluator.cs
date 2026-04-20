using Bobail.Application.Interfaces.Services;
using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;
using Bobail.Infrastructure.Bots;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bobail.Training.Simulation;

public sealed class WeightsFitnessEvaluator
{
    private readonly GameSimulator _simulator = new();
    private readonly TrainingSettings _settings;
    private readonly EvaluationWeights _baselineWeights = new();

    public WeightsFitnessEvaluator(TrainingSettings settings)
    {
        _settings = settings;
    }

    public double EvaluateFitness(EvaluationWeights weights)
    {
        double totalFitness = 0;

        totalFitness += EvaluateMatchup(
            weights,
            _settings.EasyGamesPerGenome,
            () => new EasyBotStrategy(),
            difficultyMultiplier: 1.0);

        totalFitness += EvaluateMatchup(
            weights,
            _settings.MediumGamesPerGenome,
            CreateBaselineMediumBot,
            difficultyMultiplier: 1.6);

        return totalFitness;
    }

    private double EvaluateMatchup(
        EvaluationWeights weights,
        int gamesPerGenome,
        Func<IBotStrategy> baselineFactory,
        double difficultyMultiplier)
    {
        double fitness = 0;

        for (int gameIndex = 0; gameIndex < gamesPerGenome; gameIndex++)
        {
            bool candidateStarts = gameIndex % 2 == 0;
            var candidateBot = CreateHardBot(weights);
            var baselineBot = baselineFactory();

            var result = candidateStarts
                ? _simulator.PlayGame(candidateBot, baselineBot, _settings.MaxTurnsPerGame)
                : _simulator.PlayGame(baselineBot, candidateBot, _settings.MaxTurnsPerGame);

            var candidateColor = candidateStarts ? PlayerColor.Red : PlayerColor.Green;
            fitness += ScoreResult(result, candidateColor, difficultyMultiplier);
        }

        return fitness;
    }

    private static HardBotStrategy CreateHardBot(EvaluationWeights weights)
    {
        var evaluator = new HardBoardEvaluator(weights);
        return new HardBotStrategy(evaluator, NullLogger<HardBotStrategy>.Instance);
    }

    private IBotStrategy CreateBaselineMediumBot()
    {
        var evaluator = new MediumBoardEvaluator(_baselineWeights);
        return new MediumBotStrategy(evaluator, NullLogger<MediumBotStrategy>.Instance);
    }

    private static double ScoreResult(GameResult result, PlayerColor candidateColor, double difficultyMultiplier)
    {
        int turns = Math.Max(1, result.Turns);

        if (result.Winner == candidateColor)
            return difficultyMultiplier * (1000.0 / turns);

        return -difficultyMultiplier * (1000.0 / turns);
    }
}
