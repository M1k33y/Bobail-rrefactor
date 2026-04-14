using Bobail.Training.Simulation;
using GeneticSharp;

namespace Bobail.Training.Genetics;

public sealed class EvaluationWeightsFitness : IFitness
{
    private readonly WeightsFitnessEvaluator _fitnessEvaluator;

    public EvaluationWeightsFitness(WeightsFitnessEvaluator fitnessEvaluator)
    {
        _fitnessEvaluator = fitnessEvaluator;
    }

    public double Evaluate(IChromosome chromosome)
    {
        var weightsChromosome = (EvaluationWeightsChromosome)chromosome;
        return _fitnessEvaluator.EvaluateFitness(weightsChromosome.ToWeights());
    }
}
