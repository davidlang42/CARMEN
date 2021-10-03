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
                if (requirement is ICriteriaRequirement cr)
                    score -= CostToWeight(cr.ExistingRoleCost, cr.SuitabilityWeight) * CountRoles(applicant, cr.Criteria, role);
            }
            return score / max;
        }

        /// <summary>Must be the inverse of <see cref="WeightToCost(double, double)"/></summary>
        protected static double CostToWeight(double cost, double suitability_weight)
            => -cost * suitability_weight / 100;

        /// <summary>Must be the inverse of <see cref="CostToWeight(double, double)"/></summary>
        protected static double WeightToCost(double neuron_weight, double suitability_weight)
            => -neuron_weight / suitability_weight * 100;
    }
}
