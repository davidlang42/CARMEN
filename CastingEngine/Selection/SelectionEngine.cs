using Carmen.CastingEngine.Audition;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Selection
{
    /// <summary>
    /// The abstract base class of most ISelectionEngine based engines
    /// </summary>
    public abstract class SelectionEngine : ISelectionEngine
    {
        /// <summary>A list of available selection engines</summary>
        public static readonly Type[] Implementations = new[] {
            typeof(HybridPairsSatEngine), // default
            typeof(ChunkedPairsSatEngine),
            typeof(BestPairsSatEngine),
            typeof(TopPairsSatEngine),
            typeof(ThreesACrowdSatEngine),
            typeof(HeuristicSelectionEngine),
            typeof(RankDifferenceSatEngine)
        };

        public IAuditionEngine AuditionEngine { get; init; }

        /// <summary>If null, order by OverallAbility</summary>
        protected Criteria? castNumberOrderBy { get; init; }
        protected ListSortDirection castNumberOrderDirection { get; init; }
        protected AlternativeCast[] alternativeCasts { get; init; }

        public abstract Task BalanceAlternativeCasts(IEnumerable<Applicant> applicants, IEnumerable<SameCastSet> same_cast_sets);

        public SelectionEngine(IAuditionEngine audition_engine, AlternativeCast[] alternative_casts, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction)
        {
            AuditionEngine = audition_engine;
            alternativeCasts = alternative_casts;
            castNumberOrderBy = cast_number_order_by;
            castNumberOrderDirection = cast_number_order_direction;
        }

        /// <summary>Default implementation returns an average of the suitability for each individual requirement</summary>
        public virtual double SuitabilityOf(Applicant applicant, CastGroup cast_group)
        {
            var sub_suitabilities = cast_group.Requirements.Select(req => AuditionEngine.SuitabilityOf(applicant, req)).DefaultIfEmpty();
            return sub_suitabilities.Average();
        }

        /// <summary>Default implementation returns an average of the suitability for each individual requirement</summary>
        public virtual double SuitabilityOf(Applicant applicant, Tag tag)
        {
            var sub_suitabilities = tag.Requirements.Select(req => AuditionEngine.SuitabilityOf(applicant, req)).DefaultIfEmpty();
            return sub_suitabilities.Average();
        }

        /// <summary>Select applicants into Cast Groups, respecting those already selected, by calculating the remaining count for each group,
        /// then taking the applicants with the highest overall ability until all cast groups are filled or there are no more applicants.
        /// If an applicant is eligible for more than one cast group, they will be allocated to the first one in order which has an available spot.
        /// NOTE: This currently ignores the suitability of the applicant for the Cast Group, only checking that the minimum requirements are satisfied,
        /// but this could be improved in the future. If it is, the current behaviour should be grandfathered into HeuristicSelectionEngine</summary>
        public async Task SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups)
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
            var applicants_in_order = await applicants.Where(a => !a.IsAccepted).OrderByDescending(a => AuditionEngine.OverallAbility(a)).ToArrayAsync();
            foreach (var applicant in applicants_in_order)
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
            foreach (var cg in remaining_groups.Keys.InOrder())
            {
                if (cg.Requirements.All(r => r.IsSatisfiedBy(applicant)))
                    return cg;
            }
            return null;
        }

        /// <summary>Default implementation allocates cast numbers to accepted applicants based on the requested order.
        /// Members of cast groups which alternate casts are given the first available cast number for their alternative
        /// cast, leaving gaps which are later filled from the bottom up.</summary>
        public async Task AllocateCastNumbers(IEnumerable<Applicant> applicants)
        {
            var cast_numbers = new CastNumberSet();
            // find cast numbers which are already set
            foreach (var applicant in applicants)
            {
                if (applicant.CastGroup != null)
                {
                    if (applicant.CastGroup.AlternateCasts != (applicant.AlternativeCast != null))
                        throw new ApplicationException($"Applicant '{applicant.FirstName} {applicant.LastName}' is in a cast group with Alternate Casts"
                            + $" set to {applicant.CastGroup.AlternateCasts}, but {(applicant.AlternativeCast != null ? "has" : "doesn't have")} an alternative cast.");
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
            var applicants_in_order = await CastNumberingOrder (applicants.Where(a => a.IsAccepted)).ToArrayAsync();
            foreach (var applicant in applicants_in_order)
            {
                if (applicant.CastNumber == null)
                    applicant.CastNumber = await Task.Run(() => cast_numbers.AddNextAvailable(applicant.AlternativeCast, applicant.CastGroup!)); // not null because IsAccepted
            }
        }

        /// <summary>Applies tags in an order that ensures any tags which depend on other tags through requirements
        /// are processed first. This assumes there are no circular dependency between tags.</summary>
        public async Task ApplyTags(IEnumerable<Applicant> applicants, IEnumerable<Tag> tags)
        {
            // list all tags referenced by each tag
            var tag_references = new Dictionary<Tag, HashSet<Tag>>();
            foreach (var tag in tags)
            {
                var references = tag.Requirements
                    .SelectMany(r => r.References()).Concat(tag.Requirements)
                    .OfType<TagRequirement>().Select(tr => tr.RequiredTag)
                    .ToHashSet();
                tag_references.Add(tag, references);
            }
            // check that all referenced tags are included in this ApplyTags() call
            foreach (var (tag, references) in tag_references)
                foreach (var ref_tag in references)
                    if (!tag_references.ContainsKey(ref_tag))
                        throw new ArgumentException($"Tag '{ref_tag.Name}' is referenced by '{tag.Name}' but was not included in the ApplyTags() call.");
            // allocate tags which aren't dependant on those remaining
            bool any_change;
            while (tag_references.Any())
            {
                any_change = false;
                foreach (var (tag, references) in tag_references)
                {
                    if (!references.Any(t => tag_references.ContainsKey(t)))
                    {
                        await ApplyTag(applicants, tag);
                        tag_references.Remove(tag);
                        any_change = true;
                    }
                }
                if (!any_change)
                    throw new ArgumentException("Cannot apply tags with circular dependency between tags.");
            } 
        }

        /// <summary>Applies tags by list accepted cast who aren't in this tag, ordering them by suitability
        /// for this tag, then ordering by overall mark. Tags are applied down this list until all the
        /// cast groups have the required number or there are no more eligible cast.</summary>
        public async Task ApplyTag(IEnumerable<Applicant> applicants, Tag tag)
        {
            // In this dictionary, if a key is missing that means infinite are allowed
            var remaining = new Dictionary<(CastGroup, AlternativeCast?), uint>();
            foreach (var cbg in tag.CountByGroups)
            {
                var alternative_casts = cbg.CastGroup.AlternateCasts ? alternativeCasts : new[] { (AlternativeCast?)null };
                foreach (var alternative_cast in alternative_casts)
                    remaining.Add((cbg.CastGroup, alternative_cast), cbg.Count);
            }
            // Subtract cast already allocated to tags
            foreach (var applicant in applicants)
            {
                if (applicant.CastGroup == null)
                {
                    applicant.Tags.Remove(tag); // not accepted, therefore remove tag
                    tag.Members.Remove(applicant);
                }
                else
                {
                    if (applicant.CastGroup.AlternateCasts != (applicant.AlternativeCast != null))
                        throw new ApplicationException($"Applicant '{applicant.FirstName} {applicant.LastName}' is in a cast group with Alternate Casts"
                            + $" set to {applicant.CastGroup.AlternateCasts}, but {(applicant.AlternativeCast != null ? "has" : "doesn't have")} an alternative cast.");
                    if (applicant.Tags.Contains(tag))
                    {
                        var key = (applicant.CastGroup, applicant.AlternativeCast);
                        if (remaining.TryGetValue(key, out var remaining_count) && remaining_count != 0)
                            remaining[key] = remaining_count - 1;
                    }
                }
            }
            // Don't automatically apply tags with no requirements
            if (tag.Requirements.Count == 0)
                return; // but only return after tags have been removed from rejected applicants
            // Apply tags to accepted applicants in order of suitability
            var prioritised_applicants = await applicants.Where(a => a.IsAccepted)
                .Where(a => !a.Tags.Contains(tag))
                .Where(a => tag.Requirements.All(r => r.IsSatisfiedBy(a)))
                .OrderByDescending(a => SuitabilityOf(a, tag))
                .ThenByDescending(a => AuditionEngine.OverallAbility(a))
                .ToArrayAsync();
            foreach (var applicant in prioritised_applicants) 
            {
                var key = (applicant.CastGroup!, applicant.AlternativeCast); // not null because IsAccepted
                if (!remaining.TryGetValue(key, out var remaining_count))
                {
                    // no limit on this cast group
                    applicant.Tags.Add(tag);
                    tag.Members.Add(applicant);
                }
                else if (remaining_count > 0)
                {
                    // limited, but space remaining for this cast group
                    applicant.Tags.Add(tag);
                    tag.Members.Add(applicant);
                    remaining[key] = remaining_count - 1;
                }
            }
        }

        /// <summary>Orders the applicants based on the requested cast numbering order</summary>
        protected IEnumerable<Applicant> CastNumberingOrder(IEnumerable<Applicant> applicants)
            => (castNumberOrderBy, castNumberOrderDirection) switch
            {
                (Criteria c, ListSortDirection.Ascending) => applicants.OrderBy(a => a.MarkFor(c)),
                (Criteria c, ListSortDirection.Descending) => applicants.OrderByDescending(a => a.MarkFor(c)),
                (null, ListSortDirection.Ascending) => applicants.OrderBy(a => AuditionEngine.OverallAbility(a)),
                (null, ListSortDirection.Descending) => applicants.OrderByDescending(a => AuditionEngine.OverallAbility(a)),
                _ => throw new ApplicationException($"Sort type not handled: {castNumberOrderBy} / {castNumberOrderDirection}")
            };

        /// <summary>Detects families by matching last name</summary>
        public async Task<List<SameCastSet>> DetectFamilies(IEnumerable<Applicant> applicants)
        {
            var siblings = await Task.Run(() => applicants
                .Where(a => !string.IsNullOrEmpty(a.LastName))
                .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
                .GroupBy(a => a.LastName)
                .Select(g => g.ToArray())
                .ToArray());
            var new_same_cast_sets = new List<SameCastSet>();
            foreach (var family in siblings.Where(f => f.Length > 1))
            {
                var set = family.Select(a => a.SameCastSet).OfType<SameCastSet>().FirstOrDefault();
                if (set == null)
                {
                    set = new SameCastSet();
                    new_same_cast_sets.Add(set);
                }
                foreach (var applicant in family)
                    if (applicant.SameCastSet == null)
                    {
                        applicant.SameCastSet = set;
                        set.Applicants.Add(applicant);
                    }
            }
            return new_same_cast_sets;
        }
    }
}
