using Bobail.Application.Services.Bot;
using GeneticSharp;

namespace Bobail.Training.Genetics;

public sealed class EvaluationWeightsChromosome : ChromosomeBase
{
    private static readonly string[] GeneNames =
    {
        nameof(EvaluationWeights.ProgressWeight),
        nameof(EvaluationWeights.PathToGoalWeight),
        nameof(EvaluationWeights.ImmediateWinThreatWeight),
        nameof(EvaluationWeights.ImmediateLossThreatWeight),
        nameof(EvaluationWeights.BobailMobilityWeight),
        nameof(EvaluationWeights.ForwardMobilityWeight),
        nameof(EvaluationWeights.TrapRiskWeight),
        nameof(EvaluationWeights.OpponentPressureWeight),
        nameof(EvaluationWeights.FriendlySupportWeight),
        nameof(EvaluationWeights.DestinationQualityWeight),
        nameof(EvaluationWeights.CenterControlWeight),
        nameof(EvaluationWeights.BehindBobailFormationWeight),
        nameof(EvaluationWeights.TokenDevelopmentWeight)
    };

    private static readonly (int Min, int Max)[] GeneRanges =
    {
        (100, 1500),     // ProgressWeight
        (0, 1200),     // PathToGoalWeight
        (1000, 24_000),// ImmediateWinThreatWeight
        (4000, 30_000),// ImmediateLossThreatWeight
        (40, 1400),      // BobailMobilityWeight
        (100, 1200),     // ForwardMobilityWeight
        (200, 2000),   // TrapRiskWeight
        (0, 500),      // OpponentPressureWeight
        (0, 1200),      // FriendlySupportWeight
        (1, 900),       // DestinationQualityWeight
        (40, 1000),      // CenterControlWeight
        (40, 2500),      // BehindBobailFormationWeight
        (40, 900)       // TokenDevelopmentWeight
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

    internal static (int Min, int Max) GetGeneRange(int geneIndex)
    {
        return GeneRanges[geneIndex];
    }

    public static IReadOnlyList<(string Name, int Min, int Max)> GetGeneRangeDefinitions()
    {
        return GeneNames
            .Select((name, index) =>
            {
                var (min, max) = GeneRanges[index];
                return (name, min, max);
            })
            .ToList();
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
