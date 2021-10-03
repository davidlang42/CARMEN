using Carmen.ShowModel;
using Carmen.ShowModel.Requirements;
using System;

namespace Carmen.CastingEngine.Neural
{
    internal class SuitabilityWeightChange : IWeightChange
    {
        readonly Requirement requirement;
        readonly double newWeight;

        public IOrdered Requirement => requirement;
        public string Description => Significant
            ? $"{requirement.Name}: {newWeight:0.0} (previously {requirement.SuitabilityWeight:0.0})"
            : $"{requirement.Name}: {requirement.SuitabilityWeight:0.0}";
        public bool Significant { get; init; }

        public SuitabilityWeightChange(Requirement requirement, double new_weight)
        {
            this.requirement = requirement;
            newWeight = new_weight;
            Significant = Math.Abs(newWeight - requirement.SuitabilityWeight) > IWeightChange.MINIMUM_CHANGE;
        }

        public void Accept()
        {
            requirement.SuitabilityWeight = newWeight;
        }
    }
}
