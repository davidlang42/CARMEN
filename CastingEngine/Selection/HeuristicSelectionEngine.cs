using Carmen.CastingEngine.Audition;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Carmen.CastingEngine.Selection
{
    public class HeuristicSelectionEngine : SelectionEngine
    {
        public HeuristicSelectionEngine(IAuditionEngine audition_engine, AlternativeCast[] alternative_casts, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction)
            : base(audition_engine, alternative_casts, cast_number_order_by, cast_number_order_direction)
        { }

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
                        if (same_cast_set.Applicants.Select(a => a.AlternativeCast).OfType<AlternativeCast>().Distinct().SingleOrDefaultSafe() is AlternativeCast alternative_cast)
                            cast_index = Array.IndexOf(alternativeCasts, alternative_cast);
                        else
                            cast_index = MinimumIndex(cast_counts);
                        foreach (var unset_cast in same_cast_set.Applicants.Where(a => applicants_needing_cast.Contains(a)))
                        {
                            unset_cast.AlternativeCast = alternativeCasts[cast_index];
                            cast_counts[cast_index]++;
                            applicants_needing_cast.Remove(unset_cast);
                        }
                    }
                    // allocate remaining alternative casts
                    int next_cast = 0;
                    foreach (var applicant in CastNumberingOrder(applicants_needing_cast))
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
