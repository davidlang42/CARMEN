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
        public HeuristicAllocationEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts)
            : base(applicant_engine, alternative_casts)
        { }

        public override double SuitabilityOf(Applicant applicant, Role role)
        {
            double score = (ApplicantEngine.OverallAbility(applicant) + ApplicantEngine.MinOverallAbility) / (double)ApplicantEngine.MaxOverallAbility; // between 0 and 1 inclusive
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
        /// If a role has no requirements, select other alternative casts by matching cast number, if possible.
        /// NOTE: This does not respect existing casting, and expects it to be cleared before calling</summary>
        public override IEnumerable<Applicant> PickCast(IEnumerable<Applicant> applicants, Role role)
        {
            //TODO heuristic- this whole algorithm doesn't respect already picked, but thats what the original heuristic did, so...
            var required_cast_groups = new Dictionary<CastGroup, uint>();
            foreach (var cbg in role.CountByGroups)
                if (cbg.Count != 0)
                    required_cast_groups.Add(cbg.CastGroup, cbg.Count);
            var potential_cast_by_group = applicants
                .Where(a => a.CastGroup is CastGroup cg && required_cast_groups.ContainsKey(cg))
                .Where(a => AvailabilityOf(a, role).IsAvailable)
                .Where(a => EligibilityOf(a, role).IsEligible) //TODO heuristic- document bonus: original didn't look at eligible, but now it does
                .OrderBy(a => SuitabilityOf(a, role)) // order by suitability ascending so that the lowest suitability is at the bottom of the stack
                .GroupBy(a => a.CastGroup!)
                .ToDictionary(g => g.Key, g => new Stack<Applicant>(g));
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
    }
}
