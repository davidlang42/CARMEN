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
    public class OriginalHeuristicEngine : WeightedSumEngine, ISelectionEngine
    {
        public Criteria? CastNumberOrderBy { get; set; }
        public ListSortDirection CastNumberOrderDirection { get; set; }

        /// <summary>The original heuristic engine does things in an abnormal way,
        /// which requires these values before they would otherwise be supplied</summary>
        public OriginalHeuristicEngine(Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction)
        {
            CastNumberOrderBy = cast_number_order_by;
            CastNumberOrderDirection = cast_number_order_direction;
        }

        public void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups, uint number_of_alternative_casts)
        {
            var remaining_groups = new Dictionary<CastGroup, uint?>();
            foreach (var cast_group in cast_groups)
            {
                if (cast_group.RequiredCount is uint required)
                {
                    if (cast_group.AlternateCasts)
                        required *= number_of_alternative_casts;
                    required -= (uint)cast_group.Members.Count;
                    if (required > 0)
                        remaining_groups.Add(cast_group, required);
                }
                else
                    remaining_groups.Add(cast_group, null);
            }
            foreach (var applicant in applicants.Where(a => !a.IsAccepted).OrderByDescending(a => OverallAbility(a)))
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

        private CastGroup? NextAvailableCastGroup(Dictionary<CastGroup, uint?> remaining_groups, Applicant applicant)
        {
            foreach (var (cg, remaining_count) in remaining_groups)
            {
                if (cg.Requirements.All(r => r.IsSatisfiedBy(applicant)))
                    return cg;
            }
            return null;
        }

        public void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, IEnumerable<SameCastSet> _)
        {
            //TODO ModifiedHeuristicEngine should take into account locked sets
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
                    //TODO currently this overwrites, but it should respect
                    int next_cast = 0;
                    foreach (var applicant in Sort(applicants, CastNumberOrderBy, CastNumberOrderDirection))
                    {
                        applicant.AlternativeCast = alternative_casts[next_cast++];
                        if (next_cast == alternative_casts.Length)
                            next_cast = 0;
                    }
                }
            }
        }

        private IEnumerable<Applicant> Sort(IEnumerable<Applicant> applicants, Criteria? by, ListSortDirection direction)
            => (by, direction) switch
            {
                (Criteria c, ListSortDirection.Ascending) => applicants.OrderBy(a => a.MarkFor(c)),
                (Criteria c, ListSortDirection.Descending) => applicants.OrderByDescending(a => a.MarkFor(c)),
                (null, ListSortDirection.Ascending) => applicants.OrderBy(a => OverallAbility(a)),
                (null, ListSortDirection.Descending) => applicants.OrderByDescending(a => OverallAbility(a)),
                _ => throw new ApplicationException($"Sort not handled: {by} / {direction}")
            };

        public void AllocateCastNumbers(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, Criteria? order_by, ListSortDirection sort_direction)
        {
            var cast_numbers = new CastNumberSet();
            // find cast numbers which are already set
            foreach (var applicant in applicants)
            {
                if (applicant.CastGroup != null)
                {
                    if (applicant.CastNumber is int cast_number)
                        if (!cast_numbers.Add(cast_number, applicant.AlternativeCast, applicant.CastGroup))
                            // if add fails, this cast number has already been allocated, therefore remove it
                            applicant.CastNumber = null;
                }
                else
                {
                    // clear cast numbers of rejected applicants
                    applicant.CastNumber = null;
                }
            }
            // allocate cast numbers to those who need them
            foreach (var applicant in Sort(applicants.Where(a => a.IsAccepted), order_by, sort_direction))
            {
                if (applicant.CastNumber == null)
                    applicant.CastNumber = cast_numbers.AddNextAvailable(applicant.AlternativeCast, applicant.CastGroup!); // not null because IsAccepted
            }
        }

        //TODO ModifiedHeuristicEngine should fallback to OverallAbility if tied on tag requirements
        public void ApplyTags(IEnumerable<Applicant> applicants, IEnumerable<Tag> tags) => throw new NotImplementedException();
    }
}
