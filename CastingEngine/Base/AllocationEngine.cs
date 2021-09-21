using Carmen.CastingEngine.Allocation;
using Carmen.CastingEngine.Dummy;
using Carmen.CastingEngine.Heuristic;
using Carmen.CastingEngine.Neural;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Base
{
    /// <summary>
    /// The abstract base class of most IAllocationEngine based engines
    /// </summary>
    public abstract class AllocationEngine : IAllocationEngine
    {
        /// <summary>A list of available selection engines</summary>
        public static readonly Type[] Implementations = new[] {
            typeof(HeuristicAllocationEngine),
            typeof(NeuralAllocationEngine),
            typeof(DummyAllocationEngine), //LATER remove
        };

        public IApplicantEngine ApplicantEngine { get; init; }

        /// <summary>If true, a role requiring multiple criteria will be counted as a fractical role for each criteria,
        /// equal to 1/SQRT(CriteriasRequired). If false, a role will be counted as 1 whole role for each criteria required.</summary>
        public bool CountRolesByGeometricMean { get; set; } = true; //LATER add a setting for users to change this

        /// <summary>If true, a role requiring one criteria or another will be counted as a fractional role for each criteria,
        /// equal to 1/CriteriaOptions. If false, criteria required as SubRequirements of an OrRequirement will be ignored.</summary>
        public bool CountRolesIncludingPartialRequirements { get; set; } = true; //LATER add a setting for users to change this

        protected AlternativeCast[] alternativeCasts { get; init; }

        public abstract IEnumerable<Applicant> PickCast(IEnumerable<Applicant> applicants, Role role);
        public abstract double SuitabilityOf(Applicant applicant, Role role);

        public virtual void UserPickedCast(IEnumerable<Applicant> applicants, Role role)
        { }

        public AllocationEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts)
        {
            ApplicantEngine = applicant_engine;
            alternativeCasts = alternative_casts;
        }

        /// <summary>If true, IdealCastingOrder() will enumerate roles grouped by non-multi section, in show order.
        /// If false, roles are considered across the entire show as one group.</summary>
        public bool CastingOrderByNonMultiSection { get; set; } = false;

        /// <summary>Determines how roles are prioritised based on their requirements</summary>
        public RequirementsPriority CastingOrderByPriority { get; set; } = RequirementsPriority.AllRequirementsAtOnce;

        public enum RequirementsPriority
        {
            AllRequirementsAtOnce,
            PrimaryRequirementsFirst,
            IndividualRequirementsInOrder
        }

        public uint CastingOrderGroupThreshold { get; set; } = 8;

        /// <summary>Enumerate roles by structural segments of the show, tiered based on the priority of their requirements.
        /// Within each tier, roles are ordered by the least required cast first, then by the smallest number of eligible cast available.
        /// Roles requiring more than <see cref="CastingOrderGroupThreshold"/> cast per Cast Group are grouped together and balance cast
        /// as one operation at the end of the tier.</summary>
        public virtual IEnumerable<Role[]> IdealCastingOrder(ShowRoot show_root, Applicant[] applicants_in_cast)
        {
            // List all roles once
            var remaining_roles = show_root.ItemsInOrder().SelectMany(i => i.Roles).ToHashSet();
            // Separate show into segments based on setting
            Node[] segments = CastingOrderByNonMultiSection ? NonMultiSegments(show_root).ToArray() : new[] { show_root };
            // Cast each segment in order
            foreach (var segment in segments)
            {
                // List remaining roles in this segment
                var segment_roles = segment.ItemsInOrder().SelectMany(i => i.Roles).Where(r => remaining_roles.Contains(r)).ToHashSet();
                remaining_roles.RemoveRange(segment_roles);
                // Group segment roles based on requirement priority
                var requirement_tiers = PrioritiseByRequirements(segment_roles, CastingOrderByPriority).ToArray();
                foreach (var tier_roles in requirement_tiers)
                {
                    // Group tier roles based on required count (lowest first)
                    var roles_by_required_count = tier_roles.GroupBy(r => r.CountByGroups.Select(cbg => cbg.Count).DefaultIfEmpty().Min())
                        .OrderBy(g => g.Key)
                        .Select(g => (g.Key, g.ToHashSet()))
                        .ToArray();
                    var balance_cast_between_roles = new HashSet<Role>();
                    foreach (var (required_count, roles) in roles_by_required_count)
                    {
                        if (required_count < CastingOrderGroupThreshold)
                            // Cast roles with the least eligible applicants first
                            while (roles.Any())
                            {
                                var next_role = RoleWithLeastEligibleApplicantsAvailable(roles, applicants_in_cast);
                                roles.Remove(next_role);
                                yield return new[] { next_role }; // cast role individually
                            }
                        else
                            // Batch roles requiring groups of people together to be balance cast as one operation
                            balance_cast_between_roles.AddRange(roles);
                    }
                    if (balance_cast_between_roles.Any())
                        yield return balance_cast_between_roles.ToArray(); // balance cast roles
                }
            }
        }

        private IEnumerable<Node> NonMultiSegments(InnerNode inner)
        {
            foreach (var child in inner.Children)
            {
                if (child is Item)
                    yield return child; // items are non-multi
                else if (child is Section section && !section.SectionType.AllowMultipleRoles)
                    yield return child; // some sections are non-multi
                else
                    foreach (var child_of_child in NonMultiSegments((InnerNode)child)) // go deeper into sections which allow multi
                        yield return child_of_child;
            }
        }

        private IEnumerable<List<Role>> PrioritiseByRequirements(IEnumerable<Role> roles, RequirementsPriority priority)
        {
            if (priority == RequirementsPriority.IndividualRequirementsInOrder)
            {
                var tiers = new Dictionary<int, List<Role>>();
                var no_requirements = new List<Role>();
                foreach (var role in roles)
                {
                    if (role.Requirements.Count == 0)
                        no_requirements.Add(role);
                    else
                    {
                        var lowest_order_requirement = role.Requirements.Min(req => req.Order);
                        if (tiers.TryGetValue(lowest_order_requirement, out var existing_list))
                            existing_list.Add(role);
                        else
                            tiers.Add(lowest_order_requirement, new() { role });
                    }
                }
                return tiers.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).Concat(no_requirements.Yield());
            }
            else
            {
                var tier_count = priority switch
                {
                    RequirementsPriority.AllRequirementsAtOnce => 2,
                    RequirementsPriority.PrimaryRequirementsFirst => 3,
                    _ => throw new NotImplementedException($"Enum not handled: {priority}")
                };
                var tiers = new List<Role>[tier_count];
                foreach (var role in roles)
                {
                    if (role.Requirements.Count == 0)
                        tiers[0].Add(role); // no requirements tier
                    else if (priority == RequirementsPriority.PrimaryRequirementsFirst && role.Requirements.Any(r => r.Primary))
                        tiers[2].Add(role);
                    else
                        tiers[1].Add(role);
                }
                return tiers.Reverse();
            }
        }

        private Role RoleWithLeastEligibleApplicantsAvailable(IEnumerable<Role> roles, Applicant[] applicants)
        {
            var e = roles.GetEnumerator();
            if (!e.MoveNext())
                throw new ArgumentException("Sequence was empty.");
            Role min_role = e.Current;
            int min_count = CountEligibleApplicantsAvailable(min_role, applicants);
            while (e.MoveNext())
            {
                var current_count = CountEligibleApplicantsAvailable(e.Current, applicants);
                if (current_count < min_count)
                {
                    min_role = e.Current;
                    min_count = current_count;
                }
            }
            return min_role;
        }

        private int CountEligibleApplicantsAvailable(Role role, IEnumerable<Applicant> applicants)
            => applicants.Count(a => IsEligible(a, role) && IsAvailable(a, role));

        /// <summary>Default implementation of Balance Cast calls PickCast on the roles in order, performing no balancing</summary>
        public virtual IEnumerable<(Role, IEnumerable<Applicant>)> BalanceCast(IEnumerable<Applicant> applicants, IEnumerable<Role> roles)
        {
            foreach (var role in roles)
                yield return (role, PickCast(applicants, role));
        }

        /// <summary>Counts an applicants existing roles requiring the specificed Criteria, either directly or as SubRequirements of an AndRequirement.
        /// If <see cref="CountRolesByGeometricMean"/> is true, a role requiring multiple criteria will be counted as a fractical role for each criteria.
        /// If <see cref="CountRolesIncludingPartialRequirements"/> is true, criteria required as SubRequirements of an OrRequirement will be included.</summary>
        /// Any NOT requirements or non-criteria requirements will be ignored.</summary>
        public double CountRoles(Applicant applicant, Criteria criteria, Role? excluding_role)
        {
            double role_count = 0;
            foreach (var role in applicant.Roles.Where(r => r != excluding_role))
            {
                var counts = CriteriaCounts(role.Requirements);
                if (counts.TryGetValue(criteria, out var count))
                {
                    if (CountRolesByGeometricMean)
                        count /= Math.Sqrt(counts.Values.Sum());
                    role_count += count;
                }
            }
            return role_count;
        }

        private Dictionary<Criteria, double> CriteriaCounts(IEnumerable<Requirement> requirements)
        {
            var counts = new Dictionary<Criteria, double>();
            foreach (var requirement in requirements)
            {
                if (requirement is ICriteriaRequirement criteria_requirement)
                    counts[criteria_requirement.Criteria] = 1; // referencing the same criteria twice doesn't count as more
                else if (requirement is CombinedRequirement combined && (CountRolesIncludingPartialRequirements || combined is AndRequirement)) // treat AND sub-requirements as if they were direct requirements
                {
                    var sub_counts = CriteriaCounts(combined.SubRequirements);
                    if (combined is not AndRequirement)
                        ArithmeticMeanInPlace(sub_counts);
                    foreach (var (sub_criteria, sub_count) in sub_counts)
                        if (!counts.TryGetValue(sub_criteria, out var existing_count) || existing_count < sub_count)
                            counts[sub_criteria] = sub_count; // only keep the max value, referencing twice doesn't count as more
                }
            }
            return counts;
        }

        private void ArithmeticMeanInPlace(Dictionary<Criteria, double> values)
        {
            var total_sum = values.Values.Sum();
            foreach (var key in values.Keys)
                values[key] /= total_sum;
        }

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

        /// <summary>Same logic as <see cref="EligibilityOf"/> but shortcut to boolean result</summary>
        public bool IsEligible(Applicant applicant, Role role)
            => role.Requirements.All(req => req.IsSatisfiedBy(applicant));

        /// <summary>Determine if an applicant is available to be cast in a role
        /// (eg. already cast in the same item, an adjacent item, or within a section where AllowMultipleRoles==FALSE)</summary>
        public Availability AvailabilityOf(Applicant applicant, Role role) //LATER thoroughly unit test
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

        /// <summary>Same logic as <see cref="AvailabilityOf"/> but shortcut to boolean result</summary>
        public bool IsAvailable(Applicant applicant, Role role)
        {
            var applicant_items = applicant.Roles.Where(r => r != role).SelectMany(r => r.Items).ToHashSet();
            var role_items = role.Items.ToHashSet();
            return !applicant_items.Intersect(role_items).Any()
                && !FindCommonNonMultiSections(applicant_items, role_items).Any()
                && !FindAdjacentItems(applicant_items, role_items).Any();
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
                    previous = e.Current;
                }
            }
        }

        private InnerNode? HighestNonConsecutiveNode(Item item)
            => item.Parents().Where(n => !n.AllowConsecutiveItems).LastOrDefault();
    }
}
