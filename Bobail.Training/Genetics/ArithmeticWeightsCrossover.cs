using GeneticSharp;

namespace Bobail.Training.Genetics;

//childA = alpha * parentA + (1 - alpha) * parentB
//childB = (1 - alpha) * parentA + alpha * parentB
public sealed class ArithmeticWeightsCrossover : CrossoverBase
{
    private const double MinBlend = 0.25;
    private const double MaxBlend = 0.75;

    public ArithmeticWeightsCrossover()
        : base(2, 2)
    {
    }

    protected override IList<IChromosome> PerformCross(IList<IChromosome> parents)
    {
        ArgumentNullException.ThrowIfNull(parents);

        if (parents.Count != 2 ||
            parents[0] is not EvaluationWeightsChromosome parentA ||
            parents[1] is not EvaluationWeightsChromosome parentB)
        {
            throw new InvalidOperationException(
                $"{nameof(ArithmeticWeightsCrossover)} requires exactly two {nameof(EvaluationWeightsChromosome)} parents.");
        }

        var random = RandomizationProvider.Current;
        var childA = (EvaluationWeightsChromosome)parentA.CreateNew();
        var childB = (EvaluationWeightsChromosome)parentB.CreateNew();

        for (var geneIndex = 0; geneIndex < parentA.Length; geneIndex++)
        {
            var alpha = MinBlend + random.GetDouble() * (MaxBlend - MinBlend);
            var parentAValue = (int)parentA.GetGene(geneIndex).Value;
            var parentBValue = (int)parentB.GetGene(geneIndex).Value;

            var childAValue = Blend(parentAValue, parentBValue, alpha, geneIndex);
            var childBValue = Blend(parentAValue, parentBValue, 1 - alpha, geneIndex);

            childA.ReplaceGene(geneIndex, new Gene(childAValue));
            childB.ReplaceGene(geneIndex, new Gene(childBValue));
        }

        return [childA, childB];
    }

    private static int Blend(int parentAValue, int parentBValue, double alpha, int geneIndex)
    {
        var (min, max) = EvaluationWeightsChromosome.GetGeneRange(geneIndex);
        var value = (int)Math.Round((alpha * parentAValue) + ((1 - alpha) * parentBValue));

        return Math.Clamp(value, min, max);
    }
}
