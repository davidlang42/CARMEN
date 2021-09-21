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
        private Criteria[] criterias;

        public HeuristicAllocationEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, Criteria[] criterias)
            : base(applicant_engine, alternative_casts)
        {
            CountRolesByGeometricMean = false;
            CountRolesIncludingPartialRequirements = false;
            this.criterias = criterias;
        }

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

        /// <summary>Return a list of the best cast to pick for the role, based on suitability.
        /// If a role has no requirements, select other alternative casts by matching cast number, if possible.</summary>
        public override IEnumerable<Applicant> PickCast(IEnumerable<Applicant> applicants, Role role)
        {
            // list existing cast by cast group
            var existing_cast = new Dictionary<CastGroup, HashSet<Applicant>>();
            foreach (var group in role.Cast.GroupBy(a => a.CastGroup))
                if (group.Key is CastGroup cast_group)
                    existing_cast.Add(cast_group, group.ToHashSet());
            // calculate how many of each cast group we need to pick
            var required_cast_groups = new Dictionary<CastGroup, uint>();
            foreach (var cbg in role.CountByGroups)
            {
                var required = (int)cbg.Count;
                if (existing_cast.TryGetValue(cbg.CastGroup, out var existing_cast_in_this_group))
                {
                    var already_allocated = existing_cast_in_this_group.Count;
                    if (cbg.CastGroup.AlternateCasts)
                        already_allocated /= alternativeCasts.Length; //LATER this assumes the existing casting has the same number of each alternative cast
                    required -= already_allocated;
                }
                if (required > 0)
                    required_cast_groups.Add(cbg.CastGroup, (uint)required);
            }
            // list available cast in priority order, grouped by cast group
            var potential_cast_by_group = applicants
                .Where(a => a.CastGroup is CastGroup cg && required_cast_groups.ContainsKey(cg))
                .Where(a => !existing_cast.Values.Any(hs => hs.Contains(a)))
                .Where(a => AvailabilityOf(a, role).IsAvailable)
                .Where(a => EligibilityOf(a, role).IsEligible)
                .OrderBy(a => SuitabilityOf(a, role)) // order by suitability ascending so that the lowest suitability is at the bottom of the stack
                .ThenBy(a => ApplicantEngine.OverallAbility(a)) // then by overall ability ascending so that the lowest ability is at the bottom of the stack
                .GroupBy(a => a.CastGroup!)
                .ToDictionary(g => g.Key, g => new Stack<Applicant>(g));
            // select the required number of cast in the priority order, adding alternative cast buddies as required
            foreach (var (cast_group, potential_cast) in potential_cast_by_group)
            {
                var required = required_cast_groups[cast_group];
                for (var i = 0; i < required; i++)
                {
                    if (!potential_cast.TryPop(out var next_cast))
                        break; // no more available applicants
                    yield return next_cast;
                    if (cast_group.AlternateCasts)
                    {
                        var need_alternative_casts = alternativeCasts.Where(ac => ac != next_cast.AlternativeCast).ToHashSet();
                        if (role.Requirements.Count == 0)
                        {
                            // if not a special role, take the cast number buddies, if possible
                            while (potential_cast.FindAndRemove(a => a.CastNumber == next_cast.CastNumber) is Applicant buddy)
                            {
                                if (buddy.AlternativeCast == null || !need_alternative_casts.Remove(buddy.AlternativeCast))
                                    throw new ApplicationException($"Cast Number / Alternative Cast not set correctly for {buddy}.");
                                yield return buddy;
                            }
                        }
                        // otherwise, take the next best applicant in the other casts
                        foreach (var need_alternative_cast in need_alternative_casts)
                            if (potential_cast.FindAndRemove(a => a.AlternativeCast == need_alternative_cast) is Applicant other_cast)
                                yield return other_cast;
                    }
                }
            }
        }

        /// <summary>Recommend casting section by section (for sections that directly contain items). Within each section:
        /// - first individually cast roles which require* the first primary criteria, in item order within the section
        /// - repeat for subsequent primary criterias in order
        /// - then cast all the remaining roles within the section as one balanced set
        /// (*roles count as requiring a criteria if one of their direct requirements is an Ability based requirement)
        /// </summary>
        public IEnumerable<Role[]> SimpleCastingOrder(ShowRoot show_root)
        {
            foreach (var section in ItemContainingSections(show_root))
            {
                var section_roles = section.ItemsInOrder().SelectMany(i => i.Roles).ToHashSet();
                foreach (var primary_criteria in criterias.InOrder().Where(c => c.Primary))
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
