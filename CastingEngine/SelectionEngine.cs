using Carmen.CastingEngine.Heuristic;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine
{
    /// <summary>
    /// The abstract base class of most ISelectionEngine based engines
    /// </summary>
    public abstract class SelectionEngine : ISelectionEngine
    {
        /// <summary>A list of available selection engines</summary>
        public static readonly Type[] Implementations = new[] {
            typeof(HeuristicSelectionEngine),
            typeof(DummySelectionEngine), //LATER remove
            typeof(ChunkedPairsSatEngine)
        };

        //TODO look at common arguments, eg. cast groups/ alternative casts ande decide what should be in the constructor
        public IApplicantEngine ApplicantEngine { get; init; }

        /// <summary>If null, order by OverallAbility</summary>
        private Criteria? castNumberOrderBy { get; set; }
        private ListSortDirection castNumberOrderDirection { get; set; }

        public abstract void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, IEnumerable<SameCastSet> same_cast_sets);
        public abstract void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups, uint number_of_alternative_casts);

        public SelectionEngine(IApplicantEngine applicant_engine, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction)
        {
            ApplicantEngine = applicant_engine;
            castNumberOrderBy = cast_number_order_by;
            castNumberOrderDirection = cast_number_order_direction;
        }

        /// <summary>Default implementation returns an average of the suitability for each individual requirement</summary>
        public virtual double SuitabilityOf(Applicant applicant, CastGroup cast_group)
        {
            var sub_suitabilities = cast_group.Requirements.Select(req => ApplicantEngine.SuitabilityOf(applicant, req)).DefaultIfEmpty();
            return sub_suitabilities.Average(); //TODO handle ties with OverallAbility elsewhere
        }

        /// <summary>Default implementation returns an average of the suitability for each individual requirement</summary>
        public virtual double SuitabilityOf(Applicant applicant, Tag tag)
        {
            var sub_suitabilities = tag.Requirements.Select(req => ApplicantEngine.SuitabilityOf(applicant, req)).DefaultIfEmpty();
            return sub_suitabilities.Average(); //TODO handle ties with OverallAbility elsewhere
        }

        /// <summary>Default implementation allocates cast numbers to accepted applicants based on the requested order.
        /// Members of cast groups which alternate casts are given the first available cast number for their alternative
        /// cast, leaving gaps which are later filled from the bottom up.</summary>
        public virtual void AllocateCastNumbers(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, Criteria? _, ListSortDirection __)//TODO remove these arguments
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

        /// <summary>Default implementation applies tags in a order that ensures any tags which depend on other tags through
        /// requirements are processed first. This assumes there are no circular dependency between tags.</summary>
        public virtual void ApplyTags(IEnumerable<Applicant> applicants, IEnumerable<Tag> tags, uint number_of_alternative_casts)
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
                        ApplyTag(applicants, tag, number_of_alternative_casts);
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
            var prioritised_applicants = applicants.Where(a => a.IsAccepted)
                .Where(a => !a.Tags.Contains(tag))
                .Where(a => tag.Requirements.All(r => r.IsSatisfiedBy(a)))
                .OrderByDescending(a => SuitabilityOf(a, tag))
                .ThenByDescending(a => ApplicantEngine.OverallAbility(a));
            foreach (var applicant in prioritised_applicants) 
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

        /// <summary>Orders the applicants based on the requested cast numbering order</summary>
        protected IEnumerable<Applicant> CastNumberingOrder(IEnumerable<Applicant> applicants)
            => (castNumberOrderBy, castNumberOrderDirection) switch
            {
                (Criteria c, ListSortDirection.Ascending) => applicants.OrderBy(a => a.MarkFor(c)),
                (Criteria c, ListSortDirection.Descending) => applicants.OrderByDescending(a => a.MarkFor(c)),
                (null, ListSortDirection.Ascending) => applicants.OrderBy(a => ApplicantEngine.OverallAbility(a)),
                (null, ListSortDirection.Descending) => applicants.OrderByDescending(a => ApplicantEngine.OverallAbility(a)),
                _ => throw new ApplicationException($"Sort type not handled: {castNumberOrderBy} / {castNumberOrderDirection}")
            };
    }
}
