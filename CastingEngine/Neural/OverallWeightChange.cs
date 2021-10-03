using Carmen.ShowModel;
using Carmen.ShowModel.Structure;
using System;

namespace Carmen.CastingEngine.Neural
{
    internal class OverallWeightChange : IWeightChange, IOrdered
    {
        readonly ShowRoot showRoot;
        readonly double newWeight;

        public IOrdered Requirement { get; init; }

        public string Description => Significant
            ? $"Overall Ability: {newWeight:0.0} (previously {showRoot.OverallSuitabilityWeight:0.0})"
            : $"Overall Ability: {showRoot.OverallSuitabilityWeight:0.0}";

        public bool Significant { get; init; }

        int IOrdered.Order
        {
            get => int.MinValue;
            set => throw new NotImplementedException();
        }

        public void Accept()
        {
            showRoot.OverallSuitabilityWeight = newWeight;
        }

        public OverallWeightChange(ShowRoot show_root, double new_weight)
        {
            Requirement = this;
            showRoot = show_root;
            newWeight = new_weight;
            Significant = Math.Abs(newWeight - show_root.OverallSuitabilityWeight) > IWeightChange.MINIMUM_CHANGE;
        }
    }
}
