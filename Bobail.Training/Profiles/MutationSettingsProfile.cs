namespace Bobail.Training.Profiles;

public sealed record MutationSettingsProfile(
    float BaseMutationProbability,
    float MediumMutationProbability,
    float HighMutationProbability,
    double ImprovementEpsilon);
