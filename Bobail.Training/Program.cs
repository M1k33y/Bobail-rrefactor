using Bobail.Application.Services.Bot;
using Bobail.Training.Genetics;
using Bobail.Training.Simulation;
using GeneticSharp;

var settings = new TrainingSettings
{
    EasyGamesPerGenome = 6,
    MediumGamesPerGenome = 26,
    MaxTurnsPerGame = 200,
    Generations = 200,
    PopulationMinSize = 50,
    PopulationMaxSize = 100



    // EasyGamesPerGenome = 5;
    // MediumGamesPerGenome = 20;
    // MaxTurnsPerGame = 100
    // Generations = 100;
    // PopulationMinSize = 40;
    // PopulationMaxSize = 60;
};

var fitnessEvaluator = new WeightsFitnessEvaluator(settings);
var chromosome = new EvaluationWeightsChromosome();
var population = new Population(settings.PopulationMinSize, settings.PopulationMaxSize, chromosome);
var fitness = new EvaluationWeightsFitness(fitnessEvaluator);

var ga = new GeneticAlgorithm(
    population,
    fitness,
    new NonLinearRankSelection(rankDecay: 0.99),
    new UniformCrossover(),
    new SimpleHybridMutation());

const float baseMutationProbability = 0.12f;
const float mediumMutationProbability = 0.25f;
const float highMutationProbability = 0.45f;
const double improvementEpsilon = 0.01;

double bestFitnessSoFar = double.MinValue;
double lastMeaningfulImprovementFitness = double.MinValue;
EvaluationWeights? bestWeightsSoFar = null;
int stagnantGenerations = 0;

ga.MutationProbability = baseMutationProbability;

ga.Termination = new GenerationNumberTermination(settings.Generations);
var generationTimer = System.Diagnostics.Stopwatch.StartNew();

ga.GenerationRan += (_, _) =>
{
    var generationElapsed = generationTimer.Elapsed;
    generationTimer.Restart();

    if (ga.BestChromosome is EvaluationWeightsChromosome bestChromosome)
    {
        var generationFitness = bestChromosome.Fitness.GetValueOrDefault(double.MinValue);

        if (generationFitness > bestFitnessSoFar)
        {
            bestFitnessSoFar = generationFitness;
            bestWeightsSoFar = bestChromosome.ToWeights();
        }

        if (generationFitness > lastMeaningfulImprovementFitness + improvementEpsilon)
        {
            lastMeaningfulImprovementFitness = generationFitness;
            stagnantGenerations = 0;
        }
        else
        {
            stagnantGenerations++;
        }

        ga.MutationProbability = stagnantGenerations switch
        {
            >= 8 => highMutationProbability,
            >= 3 => mediumMutationProbability,
            _ => baseMutationProbability
        };

        Console.WriteLine(
            $"Generation {ga.GenerationsNumber}: time={generationElapsed:mm\\:ss}, generationBest={bestChromosome.Fitness:F2}, globalBest={bestFitnessSoFar:F2}, stagnant={stagnantGenerations}, mutation={ga.MutationProbability:F2}, weights={bestChromosome.ToWeights()}");
        Console.WriteLine();
    }
};

Console.WriteLine("Starting Bobail weight optimization...");
ga.Start();

var finalGenerationBest = (EvaluationWeightsChromosome)ga.BestChromosome;
bestWeightsSoFar ??= finalGenerationBest.ToWeights();
bestFitnessSoFar = bestFitnessSoFar == double.MinValue
    ? finalGenerationBest.Fitness.GetValueOrDefault()
    : bestFitnessSoFar;

Console.WriteLine();
Console.WriteLine("Optimization finished.");
Console.WriteLine($"Best fitness: {bestFitnessSoFar:F2}");
Console.WriteLine($"Best weights: {bestWeightsSoFar}");
Console.WriteLine($"Final generation best fitness: {finalGenerationBest.Fitness:F2}");
Console.WriteLine($"Final generation best weights: {finalGenerationBest.ToWeights()}");
