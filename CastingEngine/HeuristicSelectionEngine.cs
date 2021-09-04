using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine
{
    public class HeuristicSelectionEngine : SelectionEngine
    {
        public HeuristicSelectionEngine(IApplicantEngine applicant_engine, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction)
            : base(applicant_engine, cast_number_order_by, cast_number_order_direction)
        { }

        //TODO summary comment
        public override void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups, uint number_of_alternative_casts)
        {
            // In this dictionary, a value of null means infinite are allowed, but if the key is missing that means no more are allowed
            var remaining_groups = new Dictionary<CastGroup, uint?>();
            // Calculate the remaining number of cast needed in each group (respecting those already accepted)
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

        //TODO summary comment
        public override void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, IEnumerable<SameCastSet> _)
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
                        applicant.AlternativeCast = alternative_casts[next_cast++];
                        if (next_cast == alternative_casts.Length)
                            next_cast = 0;
                    }
                }
            }
        }

        //TODO summary comment
        public override void AllocateCastNumbers(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, Criteria? _, ListSortDirection _)//TODO remove these arguments
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
            foreach (var applicant in CastNumberingOrder(applicants.Where(a => a.IsAccepted)))
            {
                if (applicant.CastNumber == null)
                    applicant.CastNumber = cast_numbers.AddNextAvailable(applicant.AlternativeCast, applicant.CastGroup!); // not null because IsAccepted
            }
        }

        //TODO summary comment
        public override void ApplyTags(IEnumerable<Applicant> applicants, IEnumerable<Tag> tags, uint number_of_alternative_casts)
        {
            // allocate tags sequentially because they aren't dependant on each other
            tags = tags.ToArray(); //TODO heuristic- fix this hack for concurrent modification of collecion
            foreach (var tag in tags)
                ApplyTag(applicants, tag, number_of_alternative_casts);
        }

        //TODO summary comment
        public void ApplyTag(IEnumerable<Applicant> applicants, Tag tag, uint number_of_alternative_casts)
        {
            // In this dictionary, if a key is missing that means infinite are allowed
            var remaining = tag.CountByGroups.ToDictionary(
                cbg => cbg.CastGroup,
                cbg => cbg.CastGroup.AlternateCasts ? number_of_alternative_casts * cbg.Count : cbg.Count);
            // Subtract cast already allocated to tags
            foreach (var applicant in applicants)
            {
                if (applicant.CastGroup == null)
                    applicant.Tags.Remove(tag); // not accepted, therefore remove tag
                else if (applicant.Tags.Contains(tag)
                    && remaining.TryGetValue(applicant.CastGroup, out var remaining_count)
                    && remaining_count != 0)
                    remaining[applicant.CastGroup] = remaining_count - 1;
            }
            // Apply tags to accepted applicants in order of suitability
            foreach (var applicant in applicants.Where(a => a.IsAccepted).OrderByDescending(a => SuitabilityOf(a, tag.Requirements))) //TODO heuristic- ModifiedHeuristicEngine should fallback to OverallAbility if tied on tag requirements
            {
                if (tag.Requirements.All(r => r.IsSatisfiedBy(applicant))) // cast member meets minimum requirement
                {
                    var cast_group = applicant.CastGroup!; // not null because IsAccepted
                    if (!remaining.TryGetValue(cast_group, out var remaining_count))
                        applicant.Tags.Add(tag); // no limit on this cast group
                    else if (remaining_count > 0)
                    {
                        // limited, but space remaining for this cast group
                        applicant.Tags.Add(tag);
                        remaining[cast_group] = remaining_count - 1;
                    }
                }
            }
        }
    }
}
