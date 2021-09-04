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
    public class HeuristicAllocationEngine : AllocationEngine
    {
        #region Copied from DummyEngine
        

        private double SuitabilityOf(Applicant applicant, Requirement requirement) //TODO copied from DummyEngine
            => requirement switch
            {
                AbilityRangeRequirement arr => ScaledSuitability(arr.IsSatisfiedBy(applicant), arr.ScaleSuitability, applicant.MarkFor(arr.Criteria), arr.Criteria.MaxMark),
                NotRequirement nr => 1 - SuitabilityOf(applicant, nr.SubRequirement),
                AndRequirement ar => ar.AverageSuitability ? ar.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Average()
                    : ar.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Product(), //TODO real implementation might use a different combination function (eg. average, weighted average, product, or max)
                OrRequirement or => or.AverageSuitability ? or.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Average()
                    : or.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Max(), //TODO real implementation might use a different combination function (eg. average, weighted average, product, or max)
                XorRequirement xr => xr.SubRequirements.Where(r => r.IsSatisfiedBy(applicant)).SingleOrDefaultSafe() is Requirement req ? SuitabilityOf(applicant, req) : 0,
                Requirement req => req.IsSatisfiedBy(applicant) ? 1 : 0
            };

        /// <summary>Counts top level AbilityExact/AbilityRange requirements only</summary>
        public double CountRoles(Applicant applicant, Criteria criteria, Role? excluding_role) //TODO copied from DummyEngine
            => applicant.Roles.Where(r => r != excluding_role)
            .Where(r => r.Requirements.Any(req => req is ICriteriaRequirement cr && cr.Criteria == criteria))
            .Count();

        /// <summary>Enumerates roles in item order, then by name, removing duplicates</summary>
        public IEnumerable<Role> IdealCastingOrder(IEnumerable<Item> items_in_order) //TODO copied from DummyEngine
            => items_in_order.SelectMany(i => i.Roles.OrderBy(r => r.Name)).Distinct();

        /// <summary>Calls PickCast on the roles in order, performing no balancing</summary>
        public IEnumerable<KeyValuePair<Role, IEnumerable<Applicant>>> BalanceCast(IEnumerable<Applicant> applicants, IEnumerable<Role> roles, IEnumerable<AlternativeCast> alternative_casts)
        {
            var casts = alternative_casts.ToArray();
            foreach (var role in roles)
                yield return new KeyValuePair<Role, IEnumerable<Applicant>>(role, PickCast(applicants, role, casts));
        }
        #endregion

        private double ScaledSuitability(bool in_range, bool scale_suitability, uint mark, uint max_mark) //TODO modified from DummyEngine
        {
            //TODO real implementation might not test if in range
            if (scale_suitability)
                return (double)mark / max_mark;
            else if (!in_range)
                return 0;
            else
                return 1;
        }

        public HeuristicAllocationEngine(Criteria[] criterias)
            : base(criterias)
        { }

        #region IAllocationEngine
        public double SuitabilityOf(Applicant applicant, Role role)
        {
            double score = (OverallAbility(applicant) + MinOverallAbility) / (double)MaxOverallAbility; // between 0 and 1 inclusive
            var max = 1;
            foreach (var requirement in role.Requirements)
            {
                if (requirement is ICriteriaRequirement based_on)
                {
                    score += 2 * SuitabilityOf(applicant, requirement) - 0.5 * CountRoles(applicant, based_on.Criteria, role) / 100.0;
                    max += 2;
                }
            }
            return score / max;
        }


        /// <summary>Return a list of the best cast to pick for the role, based on suitability.
        /// If a role has no requirements, select other alternative casts by matching cast number, if possible.
        /// NOTE: This does not respect existing casting, and expects it to be cleared before calling</summary>
        public IEnumerable<Applicant> PickCast(IEnumerable<Applicant> applicants, Role role, IEnumerable<AlternativeCast> alternative_casts)
        {
            alternative_casts = alternative_casts.ToArray(); //TODO maybe pass this in as an array?
            //TODO heuristic- this whole algorithm doesn't respect already picked, but thats what the original heuristic did, so...
            var required_cast_groups = new Dictionary<CastGroup, uint>();
            foreach (var cbg in role.CountByGroups)
                if (cbg.Count != 0)
                    required_cast_groups.Add(cbg.CastGroup, cbg.Count);
            var potential_cast_by_group = applicants
                .Where(a => a.CastGroup is CastGroup cg && required_cast_groups.ContainsKey(cg))
                .Where(a => AvailabilityOf(a, role).IsAvailable)
                .Where(a => EligibilityOf(a, role).IsEligible) //TODO heuristic- document bonus: original didn't look at eligible, but now it does
                .OrderBy(a => SuitabilityOf(a, role)) // order by suitability ascending so that the lowest suitability is at the bottom of the stack
                .GroupBy(a => a.CastGroup!)
                .ToDictionary(g => g.Key, g => new Stack<Applicant>(g));
            foreach (var (cast_group, potential_cast) in potential_cast_by_group)
            {
                var required = required_cast_groups[cast_group];
                for (var i = 0; i < required; i++)
                {
                    if (!potential_cast.TryPop(out var next_cast))
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
