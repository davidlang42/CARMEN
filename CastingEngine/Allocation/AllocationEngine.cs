﻿using Carmen.CastingEngine.Audition;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Allocation
{
    /// <summary>
    /// The abstract base class of most IAllocationEngine based engines
    /// </summary>
    public abstract class AllocationEngine : IAllocationEngine, IComparer<(Applicant, Role)>
    {
        /// <summary>A list of available selection engines</summary>
        public static readonly Type[] Implementations = new[] {
            typeof(RoleLearningAllocationEngine), // default
            typeof(SessionLearningAllocationEngine),
            typeof(WeightedAverageEngine),
            typeof(HeuristicAllocationEngine),
            typeof(ComplexNeuralAllocationEngine)
        };

        protected IAuditionEngine auditionEngine { get; init; }

        protected readonly AlternativeCast[] alternativeCasts;

        #region Engine parameters
        /// <summary>If true, a role requiring multiple criteria will be counted as a fractical role for each criteria,
        /// equal to 1/SQRT(CriteriasRequired). If false, a role will be counted as 1 whole role for each criteria required.</summary>
        public bool CountRolesByGeometricMean { get; set; } = true;

        /// <summary>If true, a role requiring one criteria or another will be counted as a fractional role for each criteria,
        /// equal to 1/CriteriaOptions. If false, criteria required as SubRequirements of an OrRequirement will be ignored.</summary>
        public bool CountRolesIncludingPartialRequirements { get; set; } = true;

        /// <summary>If true, <see cref="IdealCastingOrder(ShowRoot, Applicant[])"/> will enumerate roles grouped
        /// by non-multi section, in show order. If false, roles are considered across the entire show as one group.</summary>
        public bool CastingOrderByNonMultiSection { get; set; } = true;

        /// <summary>Determines how roles are prioritised based on their requirements</summary>
        public RequirementsPriority CastingOrderByPriority { get; set; } = RequirementsPriority.AllRequirementsAtOnce;

        /// <summary>Roles requiring more than this number of cast are considered group roles and may be balanced cast
        /// with other groups as one operation.</summary>
        public uint CastingOrderGroupThreshold { get; set; } = 8;

        /// <summary>If true, the availability of cast (ie. whether they are already cast) will be considered when counting
        /// the eligible cast for a role. This will cause variations in the <see cref="IdealCastingOrder(ShowRoot, Applicant[])"/>
        /// depending on previously made casting decisions.
        /// If false, only the eligibility is considered, which makes the <see cref="IdealCastingOrder(ShowRoot, Applicant[])"/>
        /// deterministic based on the roles, irrespective of previous casting decisions.</summary>
        public bool CastingOrderConsiderAvailability { get; set; } = true;
        #endregion

        public AllocationEngine(IAuditionEngine audition_engine, AlternativeCast[] alternative_casts)
        {
            auditionEngine = audition_engine;
            alternativeCasts = alternative_casts;
            overallAbility = new(a => auditionEngine.OverallAbility(a));
        }

        #region Passthrough of IAuditionEngine functions
        readonly FunctionCache<Applicant, int> overallAbility;

        /// <summary>Calculate the overall ability of an applicant
        /// NOTE: This is cached for speed, as an Applicant's abilities shouldn't change over the lifetime of a SelectionEngine</summary>
        public int OverallAbility(Applicant applicant) => overallAbility[applicant];

        /// <summary>The maximum value an applicant's overall ability can be</summary>
        public int MaxOverallAbility => auditionEngine.MaxOverallAbility;
        #endregion

        public abstract double SuitabilityOf(Applicant applicant, Role role);

        /// <summary>Default implementation orders by <see cref="SuitabilityOf(Applicant, Role)"/> descending (ascending if reversed)</summary>
        protected virtual List<Applicant> InPreferredOrder(IEnumerable<Applicant> applicants, Role role, bool reverse = false)
            => reverse ? applicants.OrderBy(a => SuitabilityOf(a, role)).ThenBy(a => auditionEngine.OverallAbility(a)).ToList()
            : applicants.OrderByDescending(a => SuitabilityOf(a, role)).ThenByDescending(a => auditionEngine.OverallAbility(a)).ToList();

        /// <summary>Default implementation does nothing</summary>
        public virtual Task UserPickedCast(IEnumerable<Applicant> applicants_picked, IEnumerable<Applicant> applicants_not_picked, Role role)
            => Task.CompletedTask;

        /// <summary>Default implementation does nothing</summary>
        public virtual Task ExportChanges() => Task.CompletedTask;

        /// <summary>Return a list of the best cast to pick for the role, based on suitability. If a role has no requirements,
        /// the default implementation selects other alternative casts by matching cast number, if possible.
        /// NOTE: If this behaviour is changed, this version should be grandfathered in HeuristicAllocationEngine</summary>
        public virtual async Task<List<Applicant>> PickCast(IEnumerable<Applicant> applicants, Role role)
            => await Task.Run(() =>
            {
                // list existing cast by cast group and cast
                var existing_cast = new Dictionary<(CastGroup?, AlternativeCast?), HashSet<Applicant>>();
                foreach (var group in role.Cast.GroupBy(a => (a.CastGroup, a.AlternativeCast)))
                    existing_cast.Add(group.Key, group.ToHashSet());
                // calculate how many of each cast group we need to pick
                var required_cast_groups = new Dictionary<(CastGroup, AlternativeCast?), uint>();
                foreach (var cbg in role.CountByGroups)
                {
                    var alternative_casts = cbg.CastGroup.AlternateCasts ? alternativeCasts : new[] { (AlternativeCast?)null };
                    foreach (var alternative_cast in alternative_casts)
                    {
                        var required = (int)cbg.Count;
                        if (existing_cast.TryGetValue((cbg.CastGroup, alternative_cast), out var existing_cast_in_this_group))
                            required -= existing_cast_in_this_group.Count;
                        if (required > 0)
                            required_cast_groups.Add((cbg.CastGroup, alternative_cast), (uint)required);
                    }
                }
                // list available cast in priority order, grouped by cast group
                var potential_cast_by_group = applicants
                    .Where(a => a.CastGroup is CastGroup cg && required_cast_groups.ContainsKey((cg, a.AlternativeCast)))
                    .Where(a => !existing_cast.Values.Any(hs => hs.Contains(a)))
                    .Where(a => IsEligible(a, role))
                    .Where(a => IsAvailable(a, role))
                    .GroupBy(a => (a.CastGroup!, a.AlternativeCast))
                    .ToDictionary(g => g.Key,
                    g => new Stack<Applicant>(InPreferredOrder(g, role, reverse: true))); // order in reverse so the lowest suitability is at the bottom of the stack
                // select the required number of cast in the priority order, prioritising alternative cast buddies of those already selected
                var picked_cast = new List<Applicant>();
                var buddy_numbers = existing_cast // existing cast
                    .Where(p => p.Key.Item1?.AlternateCasts == true) // in a cast group which alternates
                    .SelectMany(p => p.Value.Select(a => a.CastNumber)) // get cast number
                    .ToHashSet();
                foreach (var (cast_group_and_cast, potential_cast) in potential_cast_by_group)
                {
                    var required = required_cast_groups[cast_group_and_cast];
                    for (var i = 0; i < required; i++)
                    {
                        if (potential_cast.Count == 0)
                            break; // no more available applicants
                        Applicant? next_cast = null;
                        if (cast_group_and_cast.Item1.AlternateCasts // if this cast group alternates
                            && role.Requirements.Count == 0 // and not a special role
                            && potential_cast.FindAndRemove(a => a.CastNumber != null && buddy_numbers.Contains(a.CastNumber)) is Applicant buddy) // take the cast number buddies, if possible
                            next_cast = buddy;
                        next_cast ??= potential_cast.Pop();
                        picked_cast.Add(next_cast);
                        if (cast_group_and_cast.Item1.AlternateCasts)
                            buddy_numbers.Add(next_cast.CastNumber);
                    }
                }
                return picked_cast;
            });

        readonly FunctionCache<Node, Item[]> itemsInOrderCache = new();
        protected Item[] ItemsInOrderFast(Node node) => itemsInOrderCache.Get(node, node
            => node is Item item ? new[] { item } : ((InnerNode)node).Children.InOrder().SelectMany(n => ItemsInOrderFast(n)).ToArray());

        /// <summary>Enumerate roles by structural segments of the show, tiered based on the priority of their requirements.
        /// Within each tier, roles are ordered by the least required cast first, then by the smallest number of eligible cast available.
        /// Roles requiring more than <see cref="CastingOrderGroupThreshold"/> cast per Cast Group are grouped together and balance cast
        /// as one operation at the end of the tier.</summary>
        public IEnumerable<Role[]> IdealCastingOrder(ShowRoot show_root, Applicant[] applicants_in_cast)
        {
            // List all roles once
            var remaining_roles = ItemsInOrderFast(show_root).SelectMany(i => i.Roles).ToHashSet();
            // Separate show into segments based on setting
            Node[] segments = CastingOrderByNonMultiSection ? NonMultiSegments(show_root).ToArray() : new[] { show_root };
            // Cast each segment in order
            foreach (var segment in segments)
            {
                // List remaining roles in this segment
                var segment_roles = ItemsInOrderFast(segment).SelectMany(i => i.Roles.InNameOrder()).Distinct().Where(r => remaining_roles.Contains(r)).ToArray();
                remaining_roles.RemoveRange(segment_roles);
                // Group segment roles based on requirement priority
                var requirement_tiers = PrioritiseByRequirements(segment_roles, CastingOrderByPriority).ToArray();
                foreach (var tier_roles in requirement_tiers)
                {
                    // Group tier roles based on required count (lowest first)
                    var roles_by_required_count = tier_roles.GroupBy(r => r.CountByGroups.Select(cbg => cbg.Count).Sum())
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
                    {
                        // Must return roles which have a common section (other than ShowRoot)
                        var balance_roles_common_sections = balance_cast_between_roles.ToDictionary(r => r, r => r.ItemsAndSections());
                        while (balance_roles_common_sections.Any())
                        {
                            var e = balance_roles_common_sections.GetEnumerator();
                            e.MoveNext();
                            HashSet<Role> roles_with_common_section = new() { e.Current.Key };
                            HashSet<Node> common_sections = new(e.Current.Value);
                            while (e.MoveNext())
                            {
                                if (e.Current.Value.Any(s => common_sections.Contains(s)))
                                {
                                    roles_with_common_section.Add(e.Current.Key);
                                    common_sections.IntersectWith(e.Current.Value);
                                }
                            }
                            yield return roles_with_common_section.ToArray(); // balance cast roles
                            balance_roles_common_sections.RemoveRange(roles_with_common_section);
                        }
                    }
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
                for (var t = 0; t < tier_count; t++)
                    tiers[t] = new List<Role>();
                foreach (var role in roles)
                {
                    if (role.Requirements.Count == 0)
                        tiers[0].Add(role); // no requirements tier
                    else if (priority == RequirementsPriority.PrimaryRequirementsFirst && role.Requirements.Any(r => r.Primary))
                        tiers[2].Add(role);
                    else
                        tiers[1].Add(role);
                }
                return tiers.Where(t => t.Any()).Reverse();
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
            => CastingOrderConsiderAvailability ? applicants.Count(a => IsEligible(a, role) && IsAvailable(a, role)) : applicants.Count(a => IsEligible(a, role));

        /// <summary>Attempts to balance talent between roles by listing all eligible applicants for each role in order of suitability,
        /// then iterating through the roles, allowing each to take their next available applicant, removing that applicant from other
        /// role's lists if required, until all roles are fully cast. If at any point the number of available applicants left in a list
        /// is less than or equal to the number of cast still required for that role, then that role will cast its entire remaining list.
        /// This is not a bullet-proof approach, but *should* be good enough to balance general cast between roles/items and handle
        /// the edge cases caused by consecutive item clashes on the edge of non-multi sections.</summary>
        public async Task BalanceCast(IEnumerable<Applicant> applicants, IEnumerable<Role> roles)
        {
            // Make roles an array to avoid re-enumeration
            var roles_array = roles.ToArray();
            // Group applicants by cast group and cast
            var applicants_by_group = applicants.GroupBy(a => (a.CastGroup, a.AlternativeCast)).ToDictionary(g => g.Key, g => g.ToArray());
            // Determine which cast groups are required by these roles
            var cast_groups = roles_array.SelectMany(r => r.CountByGroups.Select(cbg => cbg.CastGroup)).ToHashSet();
            // BalanceCast one cast group and cast at a time
            foreach (var cast_group in cast_groups)
            {
                var alternative_casts = cast_group.AlternateCasts ? alternativeCasts : new AlternativeCast?[] { null };
                foreach (var alternative_cast in alternative_casts)
                    await BalanceCastForOneCastGroupAndCast(applicants_by_group[(cast_group, alternative_cast)], roles_array, cast_group, alternative_cast);
            }
        }

        private async Task BalanceCastForOneCastGroupAndCast(Applicant[] applicants, Role[] roles, CastGroup cast_group, AlternativeCast? alternative_cast)
        {
            // Count the remaining required cast for each role
            var required_cast = roles.Select(r => (int)r.CountFor(cast_group) - r.Cast.Count(a => a.CastGroup == cast_group && a.AlternativeCast == alternative_cast)).ToArray();
            // List the available applicants for each role
            var available_cast = await Task.Run(() => roles.Select(r => new Queue<Applicant>(
                InPreferredOrder(applicants.Where(a => IsEligible(a, r) && IsAvailable(a, r)), r))
                ).ToArray());
            // Check for a role which must be immediately cast, otherwise start at the first uncast role
            int role;
            if (RoleNeedsImmediateCasting(required_cast, available_cast) is int first_immediate_role)
                role = first_immediate_role;
            else if (FirstIndexToCast(required_cast, available_cast, 0, required_cast.Length - 1) is int first_uncast_role)
                role = first_uncast_role;
            else
                return; // nothing to do
            // Iteratively cast applicants to roles, until no more casting can be done
            while (true)
            {
                if (required_cast[role] > 0 && available_cast[role].TryDequeue(out var next_available))
                {
                    // Cast the next available applicant to this role
                    roles[role].Cast.Add(next_available);
                    next_available.Roles.Add(roles[role]);
                    required_cast[role]--;
                    // Remove the cast applicant from other available_cast lists
                    await Task.Run(() => RemoveIfNotAvailable(next_available, roles, available_cast));
                    if (RoleNeedsImmediateCasting(required_cast, available_cast) is int immediate_role)
                    {
                        // If any roles now have available_cast <= required_cast, cast them now
                        role = immediate_role;
                        continue;
                    }
                }
                // Move to the next role that needs casting
                if (!IncrementRoleToCast(required_cast, available_cast, ref role))
                    break; // If no more roles need casting, we are done
            }
        }

        private void RemoveIfNotAvailable(Applicant applicant, Role[] roles, Queue<Applicant>[] available_cast)
        {
            for (var i = 0; i < roles.Length; i++)
                if (available_cast[i].Contains(applicant) && !IsAvailable(applicant, roles[i]))
                    available_cast[i].Remove(applicant);
        }

        private static int? RoleNeedsImmediateCasting(int[] required_cast, Queue<Applicant>[] available_cast)
        {
            for (var i = 0; i < required_cast.Length; i++)
                if (required_cast[i] > 0 && available_cast[i].Count <= required_cast[i])
                    return i;
            return null;
        }

        private static bool IncrementRoleToCast(int[] required_cast, Queue<Applicant>[] available_cast, ref int role)
        {
            var next_role = FirstIndexToCast(required_cast, available_cast, role + 1, required_cast.Length - 1)
                ?? FirstIndexToCast(required_cast, available_cast, 0, role);
            if (next_role.HasValue)
            {
                role = next_role.Value;
                return true;
            }
            return false;
        }

        private static int? FirstIndexToCast(int[] required_cast, Queue<Applicant>[] available_cast, int start, int end)
        {
            for (var i = start; i <= end; i++)
                if (required_cast[i] > 0 && available_cast[i].Count > 0)
                    return i;
            return null;
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
                var counts = CriteriaCounts(role);
                if (counts.TryGetValue(criteria, out var count))
                {
                    if (CountRolesByGeometricMean)
                        count /= Math.Sqrt(counts.Values.Sum());
                    role_count += count;
                }
            }
            return role_count;
        }

        readonly FunctionCache<Role, Dictionary<Criteria, double>> criteriaCountsCache = new();
        private Dictionary<Criteria, double> CriteriaCounts(Role role) => criteriaCountsCache.Get(role, role => 
        {
            Dictionary<Criteria, double> calculate_counts(IEnumerable<Requirement> requirements)
            {
                var counts = new Dictionary<Criteria, double>();
                foreach (var requirement in requirements)
                {
                    if (requirement is ICriteriaRequirement criteria_requirement)
                        counts[criteria_requirement.Criteria] = 1; // referencing the same criteria twice doesn't count as more
                    else if (requirement is CombinedRequirement combined && (CountRolesIncludingPartialRequirements || combined is AndRequirement)) // treat AND sub-requirements as if they were direct requirements
                    {
                        var sub_counts = calculate_counts(combined.SubRequirements);
                        if (combined is not AndRequirement)
                            ArithmeticMeanInPlace(sub_counts);
                        foreach (var (sub_criteria, sub_count) in sub_counts)
                            if (!counts.TryGetValue(sub_criteria, out var existing_count) || existing_count < sub_count)
                                counts[sub_criteria] = sub_count; // only keep the max value, referencing twice doesn't count as more
                    }
                }
                return counts;
            }
            return calculate_counts(role.Requirements);
        });

        private static void ArithmeticMeanInPlace(Dictionary<Criteria, double> values)
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
        public Availability AvailabilityOf(Applicant applicant, Role role)
        {
            var applicant_items = applicant.Roles.Where(r => r != role).SelectMany(r => r.Items).ToHashSet();
            var role_items = role.Items.ToHashSet();
            return new Availability
            {
                AlreadyInItems = applicant_items.Intersect(role_items).ToArray(),
                AlreadyInNonMultiSections = FindCommonNonMultiSections(applicant_items, role_items).ToArray(),
                InAdjacentItems = FindConsecutiveItems(applicant, applicant_items, role_items).ToArray()
            };
        }

        /// <summary>Same logic as <see cref="AvailabilityOf"/> but shortcut to boolean result</summary>
        public bool IsAvailable(Applicant applicant, Role role)
        {
            var applicant_items = applicant.Roles.Where(r => r != role).SelectMany(r => r.Items).ToHashSet();
            var role_items = role.Items.ToHashSet();
            return !applicant_items.Intersect(role_items).Any()
                && !FindCommonNonMultiSections(applicant_items, role_items).Any()
                && !FindConsecutiveItems(applicant, applicant_items, role_items).Any();
        }

        private IEnumerable<NonMultiSectionItem> FindCommonNonMultiSections(HashSet<Item> applicant_items, HashSet<Item> role_items)
        {
            var role_non_multi_sections = role_items.SelectMany(i => nonMultiSections[i]).ToHashSet();
            foreach (var applicant_item in applicant_items)
                foreach (var applicant_non_multi_section in nonMultiSections[applicant_item])
                    if (role_non_multi_sections.Contains(applicant_non_multi_section))
                        yield return new NonMultiSectionItem
                        {
                            AlreadyInItem = applicant_item,
                            NonMultiSection = applicant_non_multi_section
                        };
        }

        readonly FunctionCache<Item, Section[]> nonMultiSections = new(item
            => item.Parents().OfType<Section>().Where(s => !s.SectionType.AllowMultipleRoles).ToArray());

        private IEnumerable<AdjacentItem> FindConsecutiveItems(Applicant reference_applicant, HashSet<Item> applicant_items, HashSet<Item> role_items)
        {
            var applicant_non_consecutive_nodes = applicant_items.Select(i => highestNonConsecutiveNode[i]).OfType<InnerNode>();
            var role_non_consecutive_nodes = role_items.Select(i => highestNonConsecutiveNode[i]).OfType<InnerNode>();
            var non_consecutive_nodes_to_check = applicant_non_consecutive_nodes.Intersect(role_non_consecutive_nodes);
            foreach (var non_consecutive_node in non_consecutive_nodes_to_check)
            {
                var items = ItemsInOrderFast(non_consecutive_node);
                for (var i = 1; i < items.Length; i++)
                    if (applicant_items.Contains(items[i - 1]) && role_items.Contains(items[i])
                        && !ConsecutiveItemAllowed(items[i - 1], items[i], reference_applicant))
                        yield return new AdjacentItem
                        {
                            AlreadyInItem = items[i - 1],
                            Adjacency = Adjacency.Previous,
                            AdjacentTo = items[i],
                            NonConsecutiveSection = non_consecutive_node
                        };
                    else if (role_items.Contains(items[i - 1]) && applicant_items.Contains(items[i])
                        && !ConsecutiveItemAllowed(items[i - 1], items[i], reference_applicant))
                        yield return new AdjacentItem
                        {
                            AlreadyInItem = items[i],
                            Adjacency = Adjacency.Next,
                            AdjacentTo = items[i - 1],
                            NonConsecutiveSection = non_consecutive_node
                        };
            }
        }

        private bool ConsecutiveItemAllowed(Item item1, Item item2, Applicant applicant)
            => item1.AllowedConsecutives.Intersect(item2.AllowedConsecutives).Any(c => c.IsAllowed(applicant));

        readonly FunctionCache<Item, InnerNode?> highestNonConsecutiveNode = new(item
            => item.Parents().Where(n => !n.AllowConsecutiveItems).LastOrDefault());

        #region Applicant comparison
        public ApplicantForRoleComparer ComparerFor(Role role)
            => new(this, role);

        int IComparer<(Applicant, Role)>.Compare((Applicant, Role) x, (Applicant, Role) y)
        {
            if (x.Item2 != y.Item2)
                throw new ArgumentException("Role must be common between the 2 values.");
            return Compare(x.Item1, y.Item1, x.Item2);
        }

        /// <summary>Default implementation compares applicants by calling <see cref="SuitabilityOf(Applicant, Role)"/></summary>
        public virtual int Compare(Applicant a, Applicant b, Role for_role)
        {
            if (a == null || b == null)
                throw new ArgumentNullException();
            var suit_a = SuitabilityOf(a, for_role);
            var suit_b = SuitabilityOf(b, for_role);
            if (suit_a > suit_b)
                return 1; // A > B
            else if (suit_a < suit_b)
                return -1; // A < B
            else // suit_a == suit_b
                return 0; // A == B
        }
        #endregion
    }
}
