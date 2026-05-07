using GeneticSharp;

namespace Bobail.Training.Genetics;

public sealed class SimpleHybridMutation : MutationBase
{
    private const double ResetChance = 0.2;
    private const double MinStepPercent = 0.08;
    private const double MaxStepPercent = 0.25;

    protected override void PerformMutate(IChromosome chromosome, float probability)
    {
        ArgumentNullException.ThrowIfNull(chromosome);

        if (chromosome is not EvaluationWeightsChromosome weightsChromosome)
        {
            throw new InvalidOperationException($"{nameof(SimpleHybridMutation)} only works with {nameof(EvaluationWeightsChromosome)}.");
        }

        var random = RandomizationProvider.Current;

        for (var geneIndex = 0; geneIndex < weightsChromosome.Length; geneIndex++)
        {
            if (random.GetDouble() > probability)
            {
                continue;
            }

            var (min, max) = EvaluationWeightsChromosome.GetGeneRange(geneIndex);
            var current = (int)weightsChromosome.GetGene(geneIndex).Value;

            // Usually nudge the current value; sometimes reset it to reintroduce exploration.
            var next = random.GetDouble() < ResetChance
                ? random.GetInt(min, max + 1)
                : MutateNear(current, min, max);

            weightsChromosome.ReplaceGene(geneIndex, new Gene(next));
        }
    }

    private static int MutateNear(int value, int min, int max)
    {
        var random = RandomizationProvider.Current;
        var percent = MinStepPercent + random.GetDouble() * (MaxStepPercent - MinStepPercent);
        var direction = random.GetDouble() < 0.5 ? -1 : 1;
        var step = Math.Max(1, (int)Math.Round(Math.Abs(value) * percent));

        return Math.Clamp(value + direction * step, min, max);
    }
}
