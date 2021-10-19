using Carmen.CastingEngine.Base;
using Carmen.CastingEngine.Heuristic;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    /// <summary>
    /// A basic AllocationEngine which calculates an Applicant's suitability for a role as a weighted average of the
    /// suitabilities for each requirement of the role.
    /// </summary>
    public class WeightedAverageEngine : AllocationEngine
    {
        protected readonly ShowRoot showRoot;

        public WeightedAverageEngine(IAuditionEngine audition_engine, AlternativeCast[] alternative_casts, ShowRoot show_root)
            : base(audition_engine, alternative_casts)
        {
            showRoot = show_root;
        }

        public override double SuitabilityOf(Applicant applicant, Role role)
        {
            var overall_suitability = AuditionEngine.OverallSuitability(applicant); // between 0 and 1 inclusive
            double score = 0;
            double max = 0;
            foreach (var requirement in role.Requirements)
            {
                score += requirement.SuitabilityWeight * AuditionEngine.SuitabilityOf(applicant, requirement);
                max += requirement.SuitabilityWeight;
                if (!showRoot.CommonOverallWeight.HasValue)
                {
                    score += requirement.OverallWeight * overall_suitability;
                    max += requirement.OverallWeight;
                }
            }
            var overall_weight = showRoot.CommonOverallWeight ?? 0;
            if (max == 0 && overall_weight == 0)
                overall_weight = 1; // if no requirements with non-zero weight, we should apply a non-zero weight to overall
            score += overall_weight * overall_suitability;
            max += overall_weight;
            foreach (var cr in role.Requirements.OfType<ICriteriaRequirement>())
                score -= CostToWeight(cr.ExistingRoleCost, cr.SuitabilityWeight, max) * CountRoles(applicant, cr.Criteria, role);
            if (score <= 0)
                return 0; // never return a negative suitability
            return score / max;
        }

        /// <summary>Must be the inverse of <see cref="WeightToCost(double, double)"/></summary>
        protected double CostToWeight(double cost, double suitability_weight, double suitability_weight_sum)
            => -cost * (showRoot.WeightExistingRoleCosts ? suitability_weight : suitability_weight_sum) / 100;

        /// <summary>Must be the inverse of <see cref="CostToWeight(double, double)"/></summary>
        protected double WeightToCost(double neuron_weight, double suitability_weight, double suitability_weight_sum)
            => -neuron_weight / (showRoot.WeightExistingRoleCosts ? suitability_weight : suitability_weight_sum) * 100;
    }
}
