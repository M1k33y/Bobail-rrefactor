using Bobail.Application.Services.Bot;
using GeneticSharp;

namespace Bobail.Training.Genetics;

public sealed class EvaluationWeightsChromosome : ChromosomeBase
{
    // Each gene maps 1:1 to EvaluationWeights so the optimized genome can be reused directly by the game.
    private static readonly (int Min, int Max)[] GeneRanges =
    {
        (100, 1_200),
        (0, 1_500),
        (0, 500),
        (0, 500),
        (0, 250),
        (0, 300),
        (0, 400),
        (1_000, 20_000)
    };

    public EvaluationWeightsChromosome() : base(GeneRanges.Length)
    {
        CreateGenes();
    }

    public override Gene GenerateGene(int geneIndex)
    {
        var (min, max) = GeneRanges[geneIndex];
        int value = RandomizationProvider.Current.GetInt(min, max + 1);
        return new Gene(value);
    }

    public override IChromosome CreateNew()
    {
        return new EvaluationWeightsChromosome();
    }

    public EvaluationWeights ToWeights()
    {
        var genes = GetGenes();

        return new EvaluationWeights
        {
            ProgressWeight = (int)genes[0].Value,
            EndgamePressureWeight = (int)genes[1].Value,
            FriendlyAdjacencyWeight = (int)genes[2].Value,
            OpponentAdjacencyPenaltyWeight = (int)genes[3].Value,
            CenterControlWeight = (int)genes[4].Value,
            ForwardMobilityWeight = (int)genes[5].Value,
            CorridorWeight = (int)genes[6].Value,
            ImmediateWinThreatWeight = (int)genes[7].Value
        };
    }
}
