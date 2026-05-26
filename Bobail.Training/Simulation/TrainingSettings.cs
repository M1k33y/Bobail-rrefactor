namespace Bobail.Training.Simulation;

public sealed class TrainingSettings
{
    public int EasyGamesPerGenome { get; init; } = 8;

    public int MediumGamesPerGenome { get; init; } = 6;

    public int HardGamesPerGenome { get; init; } = 6;

    public int MaxTurnsPerGame { get; init; } = 60;

    public int Generations { get; init; } = 100;

    public int PopulationMinSize { get; init; } = 10;

    public int PopulationMaxSize { get; init; } = 12;
}
