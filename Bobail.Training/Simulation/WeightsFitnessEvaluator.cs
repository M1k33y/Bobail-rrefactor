using Bobail.Application.Interfaces.Services;
using Bobail.Application.Services.Bot;
using Bobail.Domain.Games;
using Bobail.Infrastructure.Bots;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Bobail.Training.Simulation;

public sealed class WeightsFitnessEvaluator
{
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
            difficultyMultiplier: 1.3);

        return totalFitness;
    }

    private double EvaluateMatchup(
        EvaluationWeights weights,
        int gamesPerGenome,
        Func<IBotStrategy> baselineFactory,
        double difficultyMultiplier)
    {
        double fitness = 0;

        Parallel.For(
            fromInclusive: 0,
            toExclusive: gamesPerGenome,
            localInit: static () => new ThreadLocalSimulationState(new GameSimulator()),
            body: (gameIndex, _, localState) =>
            {
                bool candidateStarts = gameIndex % 2 == 0;
                var candidateBot = CreateHardBot(weights);
                var baselineBot = baselineFactory();

                var result = candidateStarts
                    ? localState.Simulator.PlayGame(candidateBot, baselineBot, _settings.MaxTurnsPerGame)
                    : localState.Simulator.PlayGame(baselineBot, candidateBot, _settings.MaxTurnsPerGame);

                var candidateColor = candidateStarts ? PlayerColor.Red : PlayerColor.Green;
                localState.Fitness += ScoreResult(result, candidateColor, difficultyMultiplier);

                return localState;
            },
            localFinally: localState => AddThreadLocalFitness(ref fitness, localState.Fitness));

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

        if (result.Winner is null)
            return 0;

        return -difficultyMultiplier * (1000.0 / turns);
    }

    private static void AddThreadLocalFitness(ref double totalFitness, double localFitness)
    {
        double currentTotal;
        double updatedTotal;

        do
        {
            currentTotal = totalFitness;
            updatedTotal = currentTotal + localFitness;
        }
        while (Interlocked.CompareExchange(ref totalFitness, updatedTotal, currentTotal) != currentTotal);
    }

    private sealed class ThreadLocalSimulationState(GameSimulator simulator)
    {
        public GameSimulator Simulator { get; } = simulator;

        public double Fitness { get; set; }
    }
}