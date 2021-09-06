using Carmen.CastingEngine.Base;
using Carmen.CastingEngine.Heuristic;
using Carmen.CastingEngine.SAT;
using Carmen.CastingEngine.SAT.Internal;
using Carmen.CastingEngine.Selection;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// The base class of all SAT based selection engines
    /// </summary>
    public abstract class SatEngine : SelectionEngine
    {
        public SatEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction)
            : base(applicant_engine, alternative_casts, cast_number_order_by, cast_number_order_direction)
        { }

        /// <summary>Select applicants into Cast Groups, respecting those already selected, by calculating the remaining count for each group,
        /// then taking the applicants with the highest overall ability until all cast groups are filled or there are no more applicants.
        /// NOTE: This currently ignores the suitability of the applicant for the Cast Group, only checking that the minimum requirements are satisfied,
        /// but this should be improved in the future.</summary>
        public override void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups)
        {
            //LATER add a comment about if an applicant meets the requirements for multiple cast groups, it chooses the first cast group in order which has an available spot (and implement that)
            // also make these changes back into the heuristic engine, or move this to be a common implmenetation, possibly tweak so that it chooses the cast group which an applicant is most suitable for, but think about the use case
            //LATER handle the fact that cast group requirements may not be mutually exclusive, possibly using SAT (current implementation is copied from HeuristicSelectionEngine)
            //LATER use SuitabilityOf(Applicant, CastGroup) to order applicants, handling ties with OverallAbility
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
            foreach (var (cg, _) in remaining_groups)//LATER implement order here, can probably use .Keys
            {
                if (cg.Requirements.All(r => r.IsSatisfiedBy(applicant)))
                    return cg;
            }
            return null;
        }
    }
}
