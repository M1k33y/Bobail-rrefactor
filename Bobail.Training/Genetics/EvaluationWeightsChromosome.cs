using Bobail.Application.Services.Bot;
using GeneticSharp;

namespace Bobail.Training.Genetics;

public sealed class EvaluationWeightsChromosome : ChromosomeBase
{
    // Each gene maps 1:1 to EvaluationWeights so the optimized genome can be reused directly by the game.
    private static readonly (int Min, int Max)[] GeneRanges =
    {
        (0, 1_000),
        (0, 1_000),
        (0, 25_000),
        (0, 25_000),
        (0, 500),
        (0, 500),
        (0, 1000),
        (0, 500),
        (0, 500),
        (0, 500)
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
            PathToGoalWeight = (int)genes[1].Value,
            ImmediateWinThreatWeight = (int)genes[2].Value,
            ImmediateLossThreatWeight = (int)genes[3].Value,
            BobailMobilityWeight = (int)genes[4].Value,
            ForwardMobilityWeight = (int)genes[5].Value,
            TrapRiskWeight = (int)genes[6].Value,
            OpponentPressureWeight = (int)genes[7].Value,
            FriendlySupportWeight = (int)genes[8].Value,
            DestinationQualityWeight = (int)genes[9].Value
        };
    }
}
