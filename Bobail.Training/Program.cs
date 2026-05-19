using Bobail.Application.Services.Bot;
using Bobail.Training.Genetics;
using Bobail.Training.Profiles;
using Bobail.Training.Simulation;
using GeneticSharp;

var settings = new TrainingSettings
{
    EasyGamesPerGenome = 4,
    MediumGamesPerGenome = 16,
    MaxTurnsPerGame = 200,
    Generations = 300,
    PopulationMinSize = 40,
    PopulationMaxSize = 80



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

const float baseMutationProbability = 0.10f;
const float mediumMutationProbability = 0.20f;
const float highMutationProbability = 0.30f;
const double improvementEpsilon = 0.01;

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
    generationTimer.Restart();

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
