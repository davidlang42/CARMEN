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
    /// <summary>
    /// The abstract base class of most ISelectionEngine based engines
    /// </summary>
    public abstract class SelectionEngine : ISelectionEngine
    {
        /// <summary>A list of available selection engines</summary>
        public static readonly Type[] Implementations = new[] {
            typeof(ChunkedPairsSatEngine),
            typeof(HeuristicSelectionEngine),
            typeof(TopPairsSatEngine),
            typeof(ThreesACrowdSatEngine),
            typeof(HybridPairsSatEngine),
            typeof(RankDifferenceSatEngine),
            typeof(BestPairsSatEngine)
        };

        public IAuditionEngine AuditionEngine { get; init; }

        /// <summary>If null, order by OverallAbility</summary>
        protected Criteria? castNumberOrderBy { get; init; }
        protected ListSortDirection castNumberOrderDirection { get; init; }
        protected AlternativeCast[] alternativeCasts { get; init; }

        public abstract void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, IEnumerable<SameCastSet> same_cast_sets);
        public abstract void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups);

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

        /// <summary>Default implementation allocates cast numbers to accepted applicants based on the requested order.
        /// Members of cast groups which alternate casts are given the first available cast number for their alternative
        /// cast, leaving gaps which are later filled from the bottom up.</summary>
        public virtual void AllocateCastNumbers(IEnumerable<Applicant> applicants)
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
            foreach (var applicant in CastNumberingOrder(applicants.Where(a => a.IsAccepted)))
            {
                if (applicant.CastNumber == null)
                    applicant.CastNumber = cast_numbers.AddNextAvailable(applicant.AlternativeCast, applicant.CastGroup!); // not null because IsAccepted
            }
        }

        /// <summary>Default implementation applies tags in a order that ensures any tags which depend on other tags through
        /// requirements are processed first. This assumes there are no circular dependency between tags.</summary>
        public virtual void ApplyTags(IEnumerable<Applicant> applicants, IEnumerable<Tag> tags)
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
                        ApplyTag(applicants, tag);
                        tag_references.Remove(tag);
                        any_change = true;
                    }
                }
                if (!any_change)
                    throw new ArgumentException("Cannot apply tags with circular dependency between tags.");
            } 
        }

        /// <summary>Default implementation applies tags by list accepted cast who aren't in this tag, ordering them
        /// by suitability for this tag, then ordering by overall mark. Tags are applied down this list until all the
        /// cast groups have the required number or there are no more eligible cast.</summary>
        public void ApplyTag(IEnumerable<Applicant> applicants, Tag tag)
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
                    applicant.Tags.Remove(tag); // not accepted, therefore remove tag
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
            var prioritised_applicants = applicants.Where(a => a.IsAccepted)
                .Where(a => !a.Tags.Contains(tag))
                .Where(a => tag.Requirements.All(r => r.IsSatisfiedBy(a)))
                .OrderByDescending(a => SuitabilityOf(a, tag))
                .ThenByDescending(a => AuditionEngine.OverallAbility(a));
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
        public void DetectFamilies(IEnumerable<Applicant> applicants, out List<SameCastSet> new_same_cast_sets)
        {
            new_same_cast_sets = new();
            var siblings = applicants
                .Where(a => !string.IsNullOrEmpty(a.LastName))
                .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
                .GroupBy(a => a.LastName)
                .Select(g => g.ToArray())
                .ToArray();
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
        }
    }
}
