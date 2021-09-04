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
    public class OriginalHeuristicEngine : WeightedSumEngine, ISelectionEngine, IAllocationEngine, ICastingEngine
    {
        #region ISelectionEngine
        public Criteria? CastNumberOrderBy { get; set; }
        public ListSortDirection CastNumberOrderDirection { get; set; }

        /// <summary>The original heuristic engine does things in an abnormal way,
        /// which requires these values before they would otherwise be supplied</summary>
        public OriginalHeuristicEngine(Criteria[] criterias, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction)
            : base(criterias)
        {
            CastNumberOrderBy = cast_number_order_by; //LATER remove these, only relevant to ISelectionEngine
            CastNumberOrderDirection = cast_number_order_direction;
        }

        public void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups, uint number_of_alternative_casts)
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

        /// <summary>Find a cast group with availability for which this applicant meets the cast group's requirements</summary>
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
            //TODO (HEURISTIC) ModifiedHeuristicEngine should take into account locked sets, by process:
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
                    //TODO (HEURISTIC) currently this overwrites, but it should respect
                    int next_cast = 0;
                    foreach (var applicant in Sort(applicants_group, CastNumberOrderBy, CastNumberOrderDirection))
                    {
                        applicant.AlternativeCast = alternative_casts[next_cast++];
                        if (next_cast == alternative_casts.Length)
                            next_cast = 0;
                    }
                }
            }
        }

        /// <summary>Orders the applicants by the specified criteria</summary>
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

        public void ApplyTags(IEnumerable<Applicant> applicants, IEnumerable<Tag> tags, uint number_of_alternative_casts)
        {
            // allocate tags sequentially because they aren't dependant on each other
            tags = tags.ToArray(); //TODO (HEURISTIC) fix this hack for concurrent modification of collecion
            foreach (var tag in tags)
                ApplyTag(applicants, tag, number_of_alternative_casts);
        }

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
            foreach (var applicant in applicants.Where(a => a.IsAccepted).OrderByDescending(a => SuitabilityOf(a, tag.Requirements))) //TODO (HEURISTIC) ModifiedHeuristicEngine should fallback to OverallAbility if tied on tag requirements
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
        #endregion

        #region Copied from DummyEngine
        /// <summary>Calculate the suitability of an applicant against a set of requirements.
        /// This assumes no circular references.</summary>
        private double SuitabilityOf(Applicant applicant, IEnumerable<Requirement> requirements) //LATER copied from DummyEngine
        {
            var sub_suitabilities = requirements.Select(req => SuitabilityOf(applicant, req)).DefaultIfEmpty();
            return sub_suitabilities.Average(); //LATER real implementation might use a different combination function (eg. average, weighted average, product, or max)
        }

        private double SuitabilityOf(Applicant applicant, Requirement requirement) //LATER copied from DummyEngine
            => requirement switch
            {
                AbilityRangeRequirement arr => ScaledSuitability(arr.IsSatisfiedBy(applicant), arr.ScaleSuitability, applicant.MarkFor(arr.Criteria), arr.Criteria.MaxMark),
                NotRequirement nr => 1 - SuitabilityOf(applicant, nr.SubRequirement),
                AndRequirement ar => ar.AverageSuitability ? ar.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Average()
                    : ar.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Product(), //LATER real implementation might use a different combination function (eg. average, weighted average, product, or max)
                OrRequirement or => or.AverageSuitability ? or.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Average()
                    : or.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Max(), //LATER real implementation might use a different combination function (eg. average, weighted average, product, or max)
                XorRequirement xr => xr.SubRequirements.Where(r => r.IsSatisfiedBy(applicant)).SingleOrDefaultSafe() is Requirement req ? SuitabilityOf(applicant, req) : 0,
                Requirement req => req.IsSatisfiedBy(applicant) ? 1 : 0
            };

        private double ScaledSuitability(bool in_range, bool scale_suitability, uint mark, uint max_mark) //LATER copied from DummyEngine
        {
            //LATER real implementation might not test if in range
            if (!in_range)
                return 0;
            else if (scale_suitability)
                return mark / max_mark;
            else
                return 1;
        }

        /// <summary>Counts top level AbilityExact/AbilityRange requirements only</summary>
        public double CountRoles(Applicant applicant, Criteria criteria, Role? excluding_role) //LATER copied from DummyEngine
            => applicant.Roles.Where(r => r != excluding_role)
            .Where(r => r.Requirements.Any(req => req is ICriteriaRequirement cr && cr.Criteria == criteria))
            .Count();

        /// <summary>Enumerates roles in item order, then by name, removing duplicates</summary>
        public IEnumerable<Role> IdealCastingOrder(IEnumerable<Item> items_in_order) //LATER copied from DummyEngine
            => items_in_order.SelectMany(i => i.Roles.OrderBy(r => r.Name)).Distinct();

        /// <summary>Calls PickCast on the roles in order, performing no balancing</summary>
        public IEnumerable<KeyValuePair<Role, IEnumerable<Applicant>>> BalanceCast(IEnumerable<Applicant> applicants, IEnumerable<Role> roles, IEnumerable<AlternativeCast> alternative_casts)
        {
            var casts = alternative_casts.ToArray();
            foreach (var role in roles)
                yield return new KeyValuePair<Role, IEnumerable<Applicant>>(role, PickCast(applicants, role, casts));
        }
        #endregion

        public OriginalHeuristicEngine(Criteria[] criterias)
            : base(criterias)
        { }

        #region IAllocationEngine
        public double SuitabilityOf(Applicant applicant, Role role)
        {
            double score = (OverallAbility(applicant) + MinOverallAbility) / MaxOverallAbility; // between 0 and 1 inclusive
            var max = 1;
            foreach (var requirement in role.Requirements)
            {
                if (requirement is ICriteriaRequirement based_on)
                {
                    score += 2 * SuitabilityOf(applicant, requirement) - 0.5 * CountRoles(applicant, based_on.Criteria, role) / 100;
                    max += 2;
                }
            }
            return score / max;
        }


        /// <summary>Return a list of the best cast to pick for the role, based on suitability.
        /// If a role has no requirements, select other alternative casts by matching cast number, if possible.
        /// NOTE: This clears the current cast of the role (re-selecting is left to the caller).</summary>
        public IEnumerable<Applicant> PickCast(IEnumerable<Applicant> applicants, Role role, IEnumerable<AlternativeCast> alternative_casts)
        {
            alternative_casts = alternative_casts.ToArray(); //LATER maybe pass this in as an array?
            role.Cast.Clear(); //TODO (HEURISTIC) this whole algorithm doesn't respect already picked, but thats what the original heuristic did, so...
            var required_cast_groups = new Dictionary<CastGroup, uint>();
            foreach (var cbg in role.CountByGroups)
                if (cbg.Count != 0)
                    required_cast_groups.Add(cbg.CastGroup, cbg.Count);
            var potential_cast_by_group = applicants
                .Where(a => a.CastGroup is CastGroup cg && required_cast_groups.ContainsKey(cg))
                .Where(a => AvailabilityOf(a, role).IsAvailable)
                .Where(a => EligibilityOf(a, role).IsEligible) //TODO (HEURISTIC) document bonus: original didn't look at eligible, but now it does
                .OrderByDescending(a => SuitabilityOf(a, role))
                .GroupBy(a => a.CastGroup!)
                .ToDictionary(g => g.Key, g => new Queue<Applicant>(g));
            foreach (var (cast_group, potential_cast) in potential_cast_by_group)
            {
                var required = required_cast_groups[cast_group];
                for (var i = 0; i < required; i++)
                {
                    if (!potential_cast.TryDequeue(out var next_cast))
                        break; // no more available applicants
                    yield return next_cast;
                    if (cast_group.AlternateCasts)
                    {
                        var need_alternative_casts = alternative_casts.Where(ac => ac != next_cast.AlternativeCast).ToHashSet();
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
        #endregion

        #region Copied from IAllocationEngine
        /// <summary>Determine if an applicant is eligible to be cast in a role
        /// (ie. whether all minimum requirements of the role are met)</summary>
        public Eligibility EligibilityOf(Applicant applicant, Role role)
        {
            var requirements_not_met = new HashSet<Requirement>();
            foreach (var req in role.Requirements)
                _ = req.IsSatisfiedBy(applicant, requirements_not_met);
            return new Eligibility
            {
                RequirementsNotMet = requirements_not_met.ToArray()
            };
        }

        /// <summary>Determine if an applicant is available to be cast in a role
        /// (eg. already cast in the same item, an adjacent item, or within a section where AllowMultipleRoles==FALSE)</summary>
        Availability AvailabilityOf(Applicant applicant, Role role)//LATER thoroughly unit test
        {
            var applicant_items = applicant.Roles.Where(r => r != role).SelectMany(r => r.Items).ToHashSet();
            var role_items = role.Items.ToHashSet();
            return new Availability
            {
                AlreadyInItems = applicant_items.Intersect(role_items).ToArray(),
                AlreadyInNonMultiSections = FindCommonNonMultiSections(applicant_items, role_items).ToArray(),
                InAdjacentItems = FindAdjacentItems(applicant_items, role_items).ToArray()
            };
        }

        private IEnumerable<NonMultiSectionItem> FindCommonNonMultiSections(HashSet<Item> applicant_items, HashSet<Item> role_items)
        {
            var role_non_multi_sections = role_items.SelectMany(i => NonMultiSections(i)).ToHashSet();
            foreach (var applicant_item in applicant_items)
                foreach (var applicant_non_multi_section in NonMultiSections(applicant_item))
                    if (role_non_multi_sections.Contains(applicant_non_multi_section))
                        yield return new NonMultiSectionItem
                        {
                            AlreadyInItem = applicant_item,
                            NonMultiSection = applicant_non_multi_section
                        };
        }

        private IEnumerable<Section> NonMultiSections(Item item)
            => item.Parents().OfType<Section>().Where(s => !s.SectionType.AllowMultipleRoles);

        private IEnumerable<AdjacentItem> FindAdjacentItems(HashSet<Item> applicant_items, HashSet<Item> role_items)
        {
            var applicant_non_consecutive_nodes = applicant_items.Select(i => HighestNonConsecutiveNode(i)).OfType<InnerNode>();
            var role_non_consecutive_nodes = role_items.Select(i => HighestNonConsecutiveNode(i)).OfType<InnerNode>();
            var non_consecutive_nodes_to_check = applicant_non_consecutive_nodes.Intersect(role_non_consecutive_nodes);
            foreach (var non_consecutive_node in non_consecutive_nodes_to_check)
            {
                var e = non_consecutive_node.ItemsInOrder().GetEnumerator();
                if (!e.MoveNext())
                    yield break;
                var previous = e.Current;
                while (e.MoveNext())
                {
                    if (applicant_items.Contains(previous) && role_items.Contains(e.Current))
                        yield return new AdjacentItem
                        {
                            AlreadyInItem = previous,
                            Adjacency = Adjacency.Previous,
                            AdjacentTo = e.Current,
                            NonConsecutiveSection = non_consecutive_node
                        };
                    else if (role_items.Contains(previous) && applicant_items.Contains(e.Current))
                        yield return new AdjacentItem
                        {
                            AlreadyInItem = e.Current,
                            Adjacency = Adjacency.Next,
                            AdjacentTo = previous,
                            NonConsecutiveSection = non_consecutive_node
                        };
                }
            }
        }

        private InnerNode? HighestNonConsecutiveNode(Item item)
            => item.Parents().Where(n => !n.AllowConsecutiveItems).LastOrDefault();
        #endregion
    }
}
