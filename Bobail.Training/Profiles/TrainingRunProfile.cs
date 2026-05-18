using Bobail.Application.Services.Bot;
using Bobail.Training.Simulation;

namespace Bobail.Training.Profiles;

public sealed record TrainingRunProfile(
    string Name,
    string Difficulty,
    DateTime CreatedAtUtc,
    double BestFitness,
    int BestGeneration,
    int FinalGeneration,
    double FinalGenerationBestFitness,
    TrainingSettings Settings,
    MutationSettingsProfile MutationSettings,
    IReadOnlyList<GeneRangeProfile> GeneRanges,
    EvaluationWeights Weights);
