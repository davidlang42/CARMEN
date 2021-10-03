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
    public class WeightedAverageEngine : AllocationEngine
    {
        protected readonly ShowRoot showRoot;

        /// <summary>If true, the costs of existing roles are subtracted from the suitability for that requirement, before
        /// the weighted average applies between requirements. If false, the weighted average is calculated first,
        /// then the costs of existing roles are subtracted from the final suitability.</summary>
        public bool WeightExistingRoleCosts { get; set; } = true;

        public WeightedAverageEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, ShowRoot show_root)
            : base(applicant_engine, alternative_casts)
        {
            showRoot = show_root;
        }

        public override double SuitabilityOf(Applicant applicant, Role role)
        {
            double score = ApplicantEngine.OverallSuitability(applicant); // between 0 and 1 inclusive
            double max = showRoot.OverallSuitabilityWeight;
            foreach (var requirement in role.Requirements)
            {
                score += requirement.SuitabilityWeight * ApplicantEngine.SuitabilityOf(applicant, requirement);
                max += requirement.SuitabilityWeight;
            }
            foreach (var cr in role.Requirements.OfType<ICriteriaRequirement>())
                score -= CostToWeight(cr.ExistingRoleCost, cr.SuitabilityWeight, max) * CountRoles(applicant, cr.Criteria, role);
            if (score <= 0 || max == 0)
                return 0;
            return score / max;
        }

        /// <summary>Must be the inverse of <see cref="WeightToCost(double, double)"/></summary>
        protected double CostToWeight(double cost, double suitability_weight, double suitability_weight_sum)
            => -cost * (WeightExistingRoleCosts ? suitability_weight : suitability_weight_sum) / 100;

        /// <summary>Must be the inverse of <see cref="CostToWeight(double, double)"/></summary>
        protected double WeightToCost(double neuron_weight, double suitability_weight, double suitability_weight_sum)
            => -neuron_weight / (WeightExistingRoleCosts ? suitability_weight : suitability_weight_sum) * 100;
    }
}
