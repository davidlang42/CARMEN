using Carmen.ShowModel;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;

namespace Carmen.CastingEngine.Neural
{
    internal class OverallVectorChange : IWeightChange
    {
        readonly Dictionary<string, double> overallWeights;
        readonly Requirement requirement;
        readonly double newWeight;

        public IOrdered Requirement { get; init; }

        public string Description => Significant
            ? $"Overall Ability (for {requirement.Name}): {newWeight:0.0} (previously {overallWeights[requirement.Name]:0.0})"
            : $"Overall Ability (for {requirement.Name}): {overallWeights[requirement.Name]:0.0}";

        public bool Significant { get; init; }

        public void Accept()
        {
            overallWeights[requirement.Name] = newWeight;
        }

        public OverallVectorChange(Dictionary<string, double> overall_weights, Requirement requirement, double new_weight)
        {
            overallWeights = overall_weights;
            Requirement = this.requirement = requirement;
            newWeight = new_weight;
            Significant = Math.Abs(newWeight - overallWeights[requirement.Name]) > IWeightChange.MINIMUM_CHANGE;
        }
    }
}
