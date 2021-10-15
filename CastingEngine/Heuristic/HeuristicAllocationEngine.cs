using Carmen.CastingEngine.Base;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Heuristic
{
    public class HeuristicAllocationEngine : AllocationEngine
    {
        readonly Criteria[] criterias;

        public HeuristicAllocationEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, Criteria[] criterias)
            : base(applicant_engine, alternative_casts)
        {
            CountRolesByGeometricMean = false;
            CountRolesIncludingPartialRequirements = false;
            this.criterias = criterias;
        }

        /// <summary>Essentially this is the same as WeightedAverageEngine, except with hardcoded values:
        /// - OverallSuitabilityWeight is always 1
        /// - Each criteria requirement's SuitabilityWeight is 2
        /// - Each criteria requirement's ExistingRoleCost is 0.5
        /// - Only direct criteria requirements are included</summary>
        public override double SuitabilityOf(Applicant applicant, Role role)
        {
            double score = ApplicantEngine.OverallSuitability(applicant); // between 0 and 1 inclusive
            var max = 1;
            foreach (var requirement in role.Requirements)
            {
                if (requirement is ICriteriaRequirement based_on)
                {
                    score += 2 * ApplicantEngine.SuitabilityOf(applicant, requirement) - 0.5 * CountRoles(applicant, based_on.Criteria, role) / 100.0;
                    max += 2;
                }
            }
            return score / max;
        }

        /// <summary>Recommend casting section by section (for sections that directly contain items). Within each section:
        /// - first individually cast roles which require* the first primary criteria, in item order within the section
        /// - repeat for subsequent primary criterias in order
        /// - then cast all the remaining roles within the section as one balanced set
        /// (*roles count as requiring a criteria if one of their direct requirements is an Ability based requirement)
        /// </summary>
        public IEnumerable<Role[]> SimpleCastingOrder(ShowRoot show_root)
        {
            var primary_criterias = criterias.InOrder().Where(c => c.Primary).ToArray();
            foreach (var section in ItemContainingSections(show_root))
            {
                var section_roles = ItemsInOrderFast(section).SelectMany(i => i.Roles).ToHashSet();
                foreach (var primary_criteria in primary_criterias)
                {
                    var criteria_roles = section_roles.Where(r => r.Requirements.OfType<ICriteriaRequirement>().Any(cr => cr.Criteria == primary_criteria)).ToArray();
                    foreach (var criteria_role in criteria_roles)
                    {
                        yield return new[] { criteria_role };
                        section_roles.Remove(criteria_role);
                    }
                }
                yield return section_roles.ToArray();
            }
        }

        private IEnumerable<InnerNode> ItemContainingSections(InnerNode inner)
        {
            if (inner.Children.OfType<Item>().Any())
                yield return inner;
            else
                foreach (var child in inner.Children.InOrder().Cast<InnerNode>())
                    foreach (var section in ItemContainingSections(child))
                        yield return section;
        }
    }
}
