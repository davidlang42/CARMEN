using Carmen.ShowModel;
using Carmen.ShowModel.Requirements;
using System;

namespace Carmen.CastingEngine.Allocation
{
    internal class OverallWeightChange : IWeightChange
    {
        readonly IOverallWeighting overallWeighting;
        readonly double newWeight;
        readonly string descriptiveName;

        public string Description => Significant
            ? $"{descriptiveName}: {newWeight:0.0} (previously {overallWeighting.OverallWeight:0.0})"
            : $"{descriptiveName}: {overallWeighting.OverallWeight:0.0}";

        public bool Significant { get; init; }

        public int Order
        {
            get => (overallWeighting as IOrdered)?.Order ?? int.MinValue;
            set => throw new NotImplementedException();
        }

        public void Accept()
        {
            overallWeighting.OverallWeight = newWeight;
        }

        public OverallWeightChange(IOverallWeighting overall_weighting, double new_weight)
        {
            newWeight = new_weight;
            overallWeighting = overall_weighting;
            descriptiveName = "Overall Ability";
            if (overall_weighting is Requirement requirement)
                descriptiveName += $" (for {requirement.Name})";
            Significant = Math.Abs(newWeight - overallWeighting.OverallWeight) > IWeightChange.MINIMUM_CHANGE;
        }
    }
}
