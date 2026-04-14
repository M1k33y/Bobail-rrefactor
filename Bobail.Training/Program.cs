using Bobail.Application.Services.Bot;
using Bobail.Training.Genetics;
using Bobail.Training.Simulation;
using GeneticSharp;

var settings = new TrainingSettings
{
    GamesPerGenome = 20,
    MaxTurnsPerGame = 60,
    Generations = 50,
    PopulationMinSize = 40,
    PopulationMaxSize = 80
};

var fitnessEvaluator = new WeightsFitnessEvaluator(settings);
var chromosome = new EvaluationWeightsChromosome();
var population = new Population(settings.PopulationMinSize, settings.PopulationMaxSize, chromosome);
var fitness = new EvaluationWeightsFitness(fitnessEvaluator);

var ga = new GeneticAlgorithm(
    population,
    fitness,
    new TournamentSelection(2),
    new UniformCrossover(),
    new UniformMutation(true));

ga.MutationProbability = 0.15f;

ga.Termination = new GenerationNumberTermination(settings.Generations);
ga.GenerationRan += (_, _) =>
{
    if (ga.BestChromosome is EvaluationWeightsChromosome bestChromosome)
    {
        Console.WriteLine(
            $"Generation {ga.GenerationsNumber}: fitness={bestChromosome.Fitness:F2}, weights={bestChromosome.ToWeights()}");
    }
};

Console.WriteLine("Starting Bobail weight optimization...");
ga.Start();

var best = (EvaluationWeightsChromosome)ga.BestChromosome;
Console.WriteLine();
Console.WriteLine("Optimization finished.");
Console.WriteLine($"Best fitness: {best.Fitness:F2}");
Console.WriteLine($"Best weights: {best.ToWeights()}");
