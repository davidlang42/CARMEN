using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Heuristic
{
    public class HeuristicSelectionEngine : SelectionEngine
    {
        public HeuristicSelectionEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction)
            : base(applicant_engine, alternative_casts, cast_number_order_by, cast_number_order_direction)
        { }

        //TODO summary comment
        public override void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups)
        {
            // In this dictionary, a value of null means infinite are allowed, but if the key is missing that means no more are allowed
            var remaining_groups = new Dictionary<CastGroup, uint?>();
            // Calculate the remaining number of cast needed in each group (respecting those already accepted)
            foreach (var cast_group in cast_groups)
            {
                if (cast_group.RequiredCount is uint required)
                {
                    if (cast_group.AlternateCasts)
                        required *= (uint)alternativeCasts.Length;
                    required -= (uint)cast_group.Members.Count;
                    if (required > 0)
                        remaining_groups.Add(cast_group, required);
                }
                else
                    remaining_groups.Add(cast_group, null);
            }
            // Allocate non-accepted applicants to cast groups, until the remaining counts are 0
            foreach (var applicant in applicants.Where(a => !a.IsAccepted).OrderByDescending(a => ApplicantEngine.OverallAbility(a)))//TODO note that this ignored suitabilityof(cast group)
            {
                if (NextAvailableCastGroup(remaining_groups, applicant) is CastGroup cg)
                {
                    applicant.CastGroup = cg;
                    if (remaining_groups[cg] is uint remaining_count)
                    {
                        if (remaining_count == 1)
                            remaining_groups.Remove(cg);
                        else
                            remaining_groups[cg] = remaining_count - 1;
                    }
                }
                if (remaining_groups.Count == 0)
                    break;
            }
        }

        /// <summary>Find a cast group with availability for which this applicant meets the cast group's requirements</summary>
        private CastGroup? NextAvailableCastGroup(Dictionary<CastGroup, uint?> remaining_groups, Applicant applicant)
        {
            foreach (var (cg, _) in remaining_groups)
            {
                if (cg.Requirements.All(r => r.IsSatisfiedBy(applicant)))
                    return cg;
            }
            return null;
        }

        //TODO summary comment
        public override void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, IEnumerable<SameCastSet> _)
        {
            //TODO heuristic- ModifiedHeuristicEngine should take into account locked sets, by process:
            // - do not buddy people within a locked set
            // - do not buddy someone in a locked set with someone in any other locked set
            // - swap casts within a buddy set afterwards, in order to meet all locked set requirements (should work as long as the above is true)

            // sort the applicants into cast groups
            foreach (var applicants_group in applicants.GroupBy(a => a.CastGroup))
            {
                if (applicants_group.Key is not CastGroup cast_group // not accepted into cast
                    || !cast_group.AlternateCasts) // non-alternating cast group
                {
                    // alternative cast not required, set to null
                    foreach (var applicant in applicants_group)
                        applicant.AlternativeCast = null;
                }
                else
                {
                    // allocate alternative cast
                    //TODO heuristic- currently this overwrites, but it should respect
                    int next_cast = 0;
                    foreach (var applicant in CastNumberingOrder(applicants_group))
                    {
                        applicant.AlternativeCast = alternativeCasts[next_cast++];
                        if (next_cast == alternativeCasts.Length)
                            next_cast = 0;
                    }
                }
            }
        }
    }
}
