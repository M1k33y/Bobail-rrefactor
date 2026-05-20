using Bobail.Application.Services.Bot;
using Bobail.Training.Genetics;
using Bobail.Training.Profiles;
using Bobail.Training.Simulation;
using GeneticSharp;

var settings = new TrainingSettings
{
    EasyGamesPerGenome = 6,
    MediumGamesPerGenome = 34,
    MaxTurnsPerGame = 200,
    Generations = 70,
    PopulationMinSize = 50,
    PopulationMaxSize = 70



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
    new NonLinearRankSelection(rankDecay: 0.97),
    new ArithmeticWeightsCrossover(),
    new SimpleHybridMutation());

const float baseMutationProbability = 0.10f;
const float mediumMutationProbability = 0.15f;
const float highMutationProbability = 0.25f;
const double improvementEpsilon = 0.01;
const int randomImmigrantStagnationThreshold = 15;
const double randomImmigrantFraction = 0.10;

double bestFitnessSoFar = double.MinValue;
double lastMeaningfulImprovementFitness = double.MinValue;
EvaluationWeights? bestWeightsSoFar = null;
int stagnantGenerations = 0;
int bestGeneration = 0;

ga.MutationProbability = baseMutationProbability;

ga.Termination = new GenerationNumberTermination(settings.Generations);
var generationTimer = System.Diagnostics.Stopwatch.StartNew();

ga.GenerationRan += (_, _) =>
{
    var generationElapsed = generationTimer.Elapsed;

    if (ga.BestChromosome is EvaluationWeightsChromosome bestChromosome)
    {
        var generationFitness = bestChromosome.Fitness.GetValueOrDefault(double.MinValue);

        if (generationFitness > bestFitnessSoFar)
        {
            bestFitnessSoFar = generationFitness;
            bestWeightsSoFar = bestChromosome.ToWeights();
            bestGeneration = ga.GenerationsNumber;
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
            >= 15 => highMutationProbability,
            >= 8 => mediumMutationProbability,
            _ => baseMutationProbability
        };

        var immigrantsInjected = stagnantGenerations >= randomImmigrantStagnationThreshold
            ? InjectRandomImmigrants(population, fitness, randomImmigrantFraction)
            : 0;

        Console.WriteLine(
            $"Generation {ga.GenerationsNumber}: time={generationElapsed:mm\\:ss}, generationBest={bestChromosome.Fitness:F2}, globalBest={bestFitnessSoFar:F2}, stagnant={stagnantGenerations}, mutation={ga.MutationProbability:F2}, immigrants={immigrantsInjected}");
        Console.WriteLine($" weights={bestChromosome.ToWeights()}");
        Console.WriteLine();
    }

    generationTimer.Restart();
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

var createdAtUtc = DateTime.UtcNow;
var profile = new TrainingRunProfile(
    Name: $"Hard_GA_{createdAtUtc:yyyyMMdd_HHmmss}",
    Difficulty: "Hard",
    CreatedAtUtc: createdAtUtc,
    BestFitness: bestFitnessSoFar,
    BestGeneration: bestGeneration,
    FinalGeneration: ga.GenerationsNumber,
    FinalGenerationBestFitness: finalGenerationBest.Fitness.GetValueOrDefault(),
    FitnessAggregation: "Per matchup: Min(candidate red score, candidate green score) / candidate games per color",
    Settings: settings,
    MutationSettings: new MutationSettingsProfile(
        baseMutationProbability,
        mediumMutationProbability,
        highMutationProbability,
        improvementEpsilon),
    GeneRanges: EvaluationWeightsChromosome.GetGeneRangeDefinitions()
        .Select(range => new GeneRangeProfile(range.Name, range.Min, range.Max))
        .ToList(),
    Weights: bestWeightsSoFar);

var trainingProjectDirectory = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
var profilePath = TrainingProfileWriter.Save(
    profile,
    Path.Combine(trainingProjectDirectory, "training-output"));

Console.WriteLine($"Saved best profile: {profilePath}");

static int InjectRandomImmigrants(
    Population population,
    IFitness fitness,
    double immigrantFraction)
{
    var chromosomes = population.CurrentGeneration.Chromosomes;

    if (chromosomes.Count <= 1)
        return 0;

    var immigrantCount = Math.Max(1, (int)Math.Round(chromosomes.Count * immigrantFraction));
    immigrantCount = Math.Min(immigrantCount, chromosomes.Count - 1);

    var replacementIndexes = chromosomes
        .Select((chromosome, index) => new { Chromosome = chromosome, Index = index })
        .OrderBy(item => item.Chromosome.Fitness ?? double.MinValue)
        .Take(immigrantCount)
        .Select(item => item.Index)
        .ToList();

    foreach (var index in replacementIndexes)
    {
        var immigrant = chromosomes[index].CreateNew();
        immigrant.Fitness = fitness.Evaluate(immigrant);
        chromosomes[index] = immigrant;
    }

    return replacementIndexes.Count;
}
