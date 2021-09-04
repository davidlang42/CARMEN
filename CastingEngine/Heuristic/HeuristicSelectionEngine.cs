using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
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

        /// <summary>Select applicants into Cast Groups, respecting those already selected, by calculating the remaining count for each group,
        /// then taking the applicants with the highest overall ability until all cast groups are filled or there are no more applicants.
        /// This intentionally ignores the suitability of the applicant for the Cast Group, only checking that the minimum requirements are satisfied.</summary>
        public override void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups)
        {
            // In this dictionary, a value of null means infinite are allowed, but if the key is missing that means no more are allowed
            var remaining_groups = new Dictionary<CastGroup, uint?>();
            // Calculate the remaining number of cast needed in each group (respecting those already accepted)
            foreach (var cast_group in cast_groups)
            {
                // Check that requirements don't include TagRequirements
                var tag_requirements = cast_group.Requirements
                    .SelectMany(r => r.References()).Concat(cast_group.Requirements)
                    .OfType<TagRequirement>();
                if (tag_requirements.Any())
                    throw new ApplicationException("Cast group requirements cannot refer to Tags.");
                // Add remaining cast to dictionary
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
            foreach (var applicant in applicants.Where(a => !a.IsAccepted).OrderByDescending(a => ApplicantEngine.OverallAbility(a)))
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

        /// <summary>Allocate applicants into alternative casts by grouping by cast group, counting the applicants already in casts,
        /// allocating the same_cast_set applicants first, then filling the rest by sorting the applicants into cast number order
        /// and putting one at a time into the cast with the current smallest count.</summary>
        public override void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, IEnumerable<SameCastSet> same_cast_sets)
        {
            same_cast_sets = same_cast_sets.ToArray(); // make it safe to enumerate repeatedly
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
                    // count existing alternative casts
                    var applicants_needing_cast = new List<Applicant>();
                    var cast_counts = new int[alternativeCasts.Length];
                    foreach (var applicant in applicants_group)
                    {
                        if (applicant.AlternativeCast is AlternativeCast ac)
                        {
                            var index = Array.IndexOf(alternativeCasts, ac);
                            if (index < 0)
                                throw new ApplicationException($"Alternative cast '{ac.Name}' not found in AlternativeCasts provided.");
                            cast_counts[index]++;
                        }
                        else
                            applicants_needing_cast.Add(applicant);
                    }
                    // allocate casts to same cast sets first
                    foreach (var same_cast_set in same_cast_sets)
                    {
                        int cast_index;
                        if (same_cast_set.Select(a => a.AlternativeCast).OfType<AlternativeCast>().Distinct().SingleOrDefaultSafe() is AlternativeCast alternative_cast)
                            cast_index = Array.IndexOf(alternativeCasts, alternative_cast);
                        else
                            cast_index = MinimumIndex(cast_counts);
                        foreach (var unset_cast in same_cast_set.Where(a => a.CastGroup == cast_group).Where(a => a.AlternativeCast == null))
                        {
                            unset_cast.AlternativeCast = alternativeCasts[cast_index];
                            cast_counts[cast_index]++;
                        }
                    }
                    // allocate remaining alternative casts
                    int next_cast = 0;
                    foreach (var applicant in CastNumberingOrder(applicants_group.Where(a => a.AlternativeCast == null)))
                    {
                        applicant.AlternativeCast = alternativeCasts[next_cast++];
                        if (next_cast == alternativeCasts.Length)
                            next_cast = 0;
                    }
                }
            }
        }

        /// <summary>Finds the index of the minimum value in the array</summary>
        private int MinimumIndex(int[] counts)
        {
            if (counts.Length == 0)
                throw new ApplicationException("Counts array is empty.");
            int min = 0;
            for (var i = 1; i < counts.Length; i++)
                if (counts[i] < counts[min])
                    min = i;
            return min;
        }
    }
}
