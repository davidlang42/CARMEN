﻿using Carmen.ShowModel.Requirements;
using System;

namespace Carmen.CastingEngine.Allocation
{
    internal class ExistingRoleCostChange : IWeightChange
    {
        readonly ICriteriaRequirement requirement;
        readonly double newCost;

        public string Description => Significant
            ? $"Each '{requirement.Name}' role reduces suitability by: {newCost:0.0}% (previously {requirement.ExistingRoleCost:0.0}%)"
            : $"Each '{requirement.Name}' role reduces suitability by: {requirement.ExistingRoleCost:0.0}%";

        public bool Significant { get; init; }

        public int Order
        {
            get => requirement.Order;
            set => throw new NotImplementedException();
        }

        public ExistingRoleCostChange(ICriteriaRequirement requirement, double new_cost)
        {
            this.requirement = requirement;
            newCost = new_cost;
            Significant = Math.Abs(newCost - requirement.ExistingRoleCost) > IWeightChange.MINIMUM_CHANGE;
        }

        public void Accept()
        {
            requirement.ExistingRoleCost = newCost;
        }
    }
}
