using Bobail.Application.Services.Bot;
using Bobail.Training.Genetics;
using Bobail.Training.Simulation;
using GeneticSharp;

var settings = new TrainingSettings
{
    EasyGamesPerGenome = 20,
    MediumGamesPerGenome = 10,
    MaxTurnsPerGame = 90,
    Generations = 60,
    PopulationMinSize = 30,
    PopulationMaxSize = 40



    //EasyGamesPerGenome = 20,
    //MediumGamesPerGenome = 80,
    //MaxTurnsPerGame = 100,
    //Generations = 100,
    //PopulationMinSize = 80,
    //PopulationMaxSize = 120
};

var fitnessEvaluator = new WeightsFitnessEvaluator(settings);
var chromosome = new EvaluationWeightsChromosome();
var population = new Population(settings.PopulationMinSize, settings.PopulationMaxSize, chromosome);
var fitness = new EvaluationWeightsFitness(fitnessEvaluator);

var ga = new GeneticAlgorithm(
    population,
    fitness,
    new NonLinearRankSelection(rankDecay: 0.97),
    new UniformCrossover(),
    new UniformMutation(true));

const float baseMutationProbability = 0.1f;
const float mediumMutationProbability = 0.18f;
const float highMutationProbability = 0.3f;
const double improvementEpsilon = 0.01;

double bestFitnessSoFar = double.MinValue;
int stagnantGenerations = 0;

ga.MutationProbability = baseMutationProbability;

ga.Termination = new GenerationNumberTermination(settings.Generations);
ga.GenerationRan += (_, _) =>
{
    if (ga.BestChromosome is EvaluationWeightsChromosome bestChromosome)
    {
        if (bestChromosome.Fitness > bestFitnessSoFar + improvementEpsilon)
        {
            bestFitnessSoFar = bestChromosome.Fitness.Value;
            stagnantGenerations = 0;
        }
        else
        {
            stagnantGenerations++;
        }

        ga.MutationProbability = stagnantGenerations switch
        {
            >= 10 => highMutationProbability,
            >= 5 => mediumMutationProbability,
            _ => baseMutationProbability
        };

        Console.WriteLine(
            $"Generation {ga.GenerationsNumber}: fitness={bestChromosome.Fitness:F2}, stagnant={stagnantGenerations}, mutation={ga.MutationProbability:F2}, weights={bestChromosome.ToWeights()}");
    }
};

Console.WriteLine("Starting Bobail weight optimization...");
ga.Start();

var best = (EvaluationWeightsChromosome)ga.BestChromosome;
Console.WriteLine();
Console.WriteLine("Optimization finished.");
Console.WriteLine($"Best fitness: {best.Fitness:F2}");
Console.WriteLine($"Best weights: {best.ToWeights()}");
