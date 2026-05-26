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
        (500, 2200),     // ProgressWeight
        (100, 900),     // PathToGoalWeight
        (6000, 30_000),// ImmediateWinThreatWeight
        (8000, 30_000),// ImmediateLossThreatWeight
        (300, 1600),      // BobailMobilityWeight
        (800, 2800),     // ForwardMobilityWeight
        (200, 2400),   // TrapRiskWeight
        (150, 1000),      // OpponentPressureWeight
        (700, 3300),      // FriendlySupportWeight
        (0, 1100),       // DestinationQualityWeight
        (0, 500),      // CenterControlWeight
        (1000, 5000),      // BehindBobailFormationWeight
        (40, 650)       // TokenDevelopmentWeight
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
