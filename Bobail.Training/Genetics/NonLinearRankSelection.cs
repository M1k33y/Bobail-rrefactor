using System.Collections.Generic;
using System.Linq;
using GeneticSharp;

namespace Bobail.Training.Genetics;

public sealed class NonLinearRankSelection : SelectionBase
{
    private readonly double _rankDecay;

    public NonLinearRankSelection(double rankDecay = 0.93) : base(2)
    {
        if (rankDecay <= 0 || rankDecay >= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(rankDecay), rankDecay, "Rank decay must be between 0 and 1.");
        }

        _rankDecay = rankDecay;
    }

    protected override IList<IChromosome> PerformSelectChromosomes(int number, Generation generation)
    {
        var rankedChromosomes = generation.Chromosomes
            .Where(c => c.Fitness.HasValue)
            .OrderByDescending(c => c.Fitness!.Value)
            .ToList();

        if (rankedChromosomes.Count == 0)
        {
            return [];
        }

        var cumulativeWeights = new double[rankedChromosomes.Count];
        double totalWeight = 0;

        for (int i = 0; i < rankedChromosomes.Count; i++)
        {
            totalWeight += Math.Pow(_rankDecay, i);
            cumulativeWeights[i] = totalWeight;
        }

        var selected = new List<IChromosome>(number);

        for (int i = 0; i < number; i++)
        {
            double pick = RandomizationProvider.Current.GetDouble(0, totalWeight);
            int selectedIndex = Array.BinarySearch(cumulativeWeights, pick);

            if (selectedIndex < 0)
            {
                selectedIndex = ~selectedIndex;
            }

            if (selectedIndex >= rankedChromosomes.Count)
            {
                selectedIndex = rankedChromosomes.Count - 1;
            }

            selected.Add(rankedChromosomes[selectedIndex]);
        }

        return selected;
    }
}
