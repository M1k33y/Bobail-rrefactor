using Bobail.Application.Services.Bot;
using GeneticSharp;

namespace Bobail.Training.Genetics;

public sealed class EvaluationWeightsChromosome : ChromosomeBase
{
    // Each gene maps 1:1 to EvaluationWeights so the optimized genome can be reused directly by the game.
    private static readonly (int Min, int Max)[] GeneRanges =
    {
        (250, 900),     // ProgressWeight
        (150, 700),     // PathToGoalWeight
        (6_000, 16_000),// ImmediateWinThreatWeight
        (8_000, 20_000),// ImmediateLossThreatWeight
        (40, 220),      // BobailMobilityWeight
        (80, 320),      // ForwardMobilityWeight
        (180, 600),     // TrapRiskWeight
        (80, 260),      // OpponentPressureWeight
        (30, 140),      // FriendlySupportWeight
        (80, 260),      // DestinationQualityWeight
        (40, 260),      // CenterControlWeight
        (40, 300),      // BehindBobailFormationWeight
        (30, 220)       // TokenDevelopmentWeight
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
            DestinationQualityWeight = (int)genes[9].Value,
            CenterControlWeight = (int)genes[10].Value,
            BehindBobailFormationWeight = (int)genes[11].Value,
            TokenDevelopmentWeight = (int)genes[12].Value
        };
    }
}
