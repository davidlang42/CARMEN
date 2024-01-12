using Carmen.CastingEngine.Allocation;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using Carmen.Desktop.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Security.Cryptography.X509Certificates;

namespace Carmen.Desktop.ViewModels
{
    public class NodeRolesOverview
    {
        Action<IEnumerable<Item>, IEnumerable<Role>> callbackAfterErrorCorrection;

        public Node Node { get; }

        public ObservableCollection<IncompleteRole> IncompleteRoles { get; }
        public ObservableCollection<CastingError> CastingErrors { get; }
        public ObservableCollection<AllowedConsecutive> AllowedConsecutives { get; }

        public bool AllowParallelCasting => Node is Section section && !section.SectionType.AllowMultipleRoles || Node is Item;
        public string? ParallelCastingToolTip => AllowParallelCasting ? null : "Parallel casting is only applicable to sections which don't allow applicants to have multiple roles within them";

        /// <summary>Node should be an Item or Section, NOT ShowRoot</summary>
        public NodeRolesOverview(Node node, AlternativeCast[] alternative_casts, IEnumerable<Applicant> cast_members, Action<IEnumerable<Item>, IEnumerable<Role>> callback_after_error_correction, IAllocationEngine engine)
        {
            callbackAfterErrorCorrection = callback_after_error_correction;
            Node = node;
            IncompleteRoles = FindIncompleteRoles(node, alternative_casts).ToObservableCollection();
            if (node is Section section)
                CastingErrors = FindSectionCastingErrors(section, cast_members, engine).ToObservableCollection();
            else if (node is Item item)
                CastingErrors = FindItemCastingErrors(item).ToObservableCollection();
            else
                throw new NotImplementedException($"Node type {node.GetType().Name} not handled.");
            AllowedConsecutives = node.ItemsInOrder().SelectMany(i => i.AllowedConsecutives).Distinct().ToObservableCollection();
        }

        /// <summary>Find the given roles in the IncompleteRoles list, and mark them as selected.
        /// Ignore any roles which are not found. Does not deselect other roles.</summary>
        public void SelectRoles(IEnumerable<Role> roles)
        {
            var roles_to_select = roles.ToHashSet();
            foreach (var incomplete_role in IncompleteRoles)
                if (roles_to_select.Contains(incomplete_role.Role))
                    incomplete_role.IsSelected = true;
        }

        private static IEnumerable<IncompleteRole> FindIncompleteRoles(Node node, AlternativeCast[] alternative_casts)
        {
            foreach (var role in node.ItemsInOrder().SelectMany(i => i.Roles).Distinct().InNameOrder())
                if (role.CastingStatus(alternative_casts) != Role.RoleStatus.FullyCast)
                    yield return new IncompleteRole(role);
        }

        /// <summary>Show section type errors and section consecutive item errors (because they aren't shown anywhere else)</summary>
        private IEnumerable<CastingError> FindSectionCastingErrors(Section section, IEnumerable<Applicant> cast_members, IAllocationEngine engine)
        {
            if (!section.CastingMeetsSectionTypeRules(cast_members, out var no_roles, out var multi_roles))
            {
                foreach (var applicant in no_roles)
                    yield return new CastingError($"{FullName.Format(applicant)} has no role in {section.Name}", right_click: NoRoleFixes(applicant, section, engine));
                foreach (var (applicant, count) in multi_roles)
                    yield return new CastingError($"{FullName.Format(applicant)} has {count} roles in {section.Name}", right_click: MultiRoleFixes(applicant, section));
            }
            if (!section.VerifyConsecutiveItems(out var section_failures))
                foreach (var failure in section_failures)
                    yield return new CastingError($"{failure.Cast.Count.Plural("applicant is", "applicants are")} in {failure.Item1.Name} and {failure.Item2.Name}",
                        right_click: ConsecutiveItemFixes(failure.Item1, failure.Item2, failure.Cast));
        }

        /// <summary>Show cast with multiple roles in this item and showroot (and section) consecutive item errors in the item (because showroot isn't visible)</summary>
        private IEnumerable<CastingError> FindItemCastingErrors(Item item)
        {
            foreach (var (applicant, count) in item.FindDuplicateCast())
                yield return new CastingError($"{FullName.Format(applicant)} has {count} roles in {item.Name}", right_click: MultiRoleFixes(applicant, item));
            foreach (var consecutive_cast in item.FindConsecutiveCast())
                foreach (var cast in consecutive_cast.Cast)
                    yield return new CastingError($"{FullName.Format(cast)} is cast in {consecutive_cast.Item1.Name} and {consecutive_cast.Item2.Name}",
                        right_click: ConsecutiveItemFixes(consecutive_cast.Item1, consecutive_cast.Item2, new[] { cast }));
        }

        private IEnumerable<(string, Action)> ConsecutiveItemFixes(Item item1, Item item2, IEnumerable<Applicant> applicants)
        {
            int count = applicants.Count();
            yield return ($"Allow {(count == 1 ? "this" : "these")} consecutive cast only", () => AllowConsecutive(item1, item2, applicants));
            yield return ("Allow all consecutive cast between these items", () => AllowConsecutive(item1, item2));
            string applicant_description;
            string item1_description;
            string item2_description;
            if (count == 1)
            {
                var applicant = applicants.First();
                applicant_description = $"{applicant.FirstName} {applicant.LastName}";
                item1_description = $"{item1.Name} ({string.Join(", ", applicant.Roles.Intersect(item1.Roles).Select(r => r.Name))})";
                item2_description = $"{item2.Name} ({string.Join(", ", applicant.Roles.Intersect(item2.Roles).Select(r => r.Name))})";
            }
            else
            {
                applicant_description = $"{count} applicants";
                item1_description = item1.Name;
                item2_description = item2.Name;
            }
            yield return ($"Remove {applicant_description} from {item1_description}", () => RemoveFromItem(item1, applicants));
            yield return ($"Remove {applicant_description} from {item2_description}", () => RemoveFromItem(item2, applicants));
        }

        private void RemoveFromItem(Item item, IEnumerable<Applicant> applicants)
        {
            var changed_roles = new HashSet<Role>();
            foreach (var applicant in applicants)
                foreach (var role in applicant.Roles.Intersect(item.Roles).ToArray())
                {
                    role.Cast.Remove(applicant);
                    applicant.Roles.Remove(role);
                    changed_roles.Add(role);
                }
            callbackAfterErrorCorrection(Enumerable.Empty<Item>(), changed_roles);
        }

        private void AllowConsecutive(Item item1, Item item2, IEnumerable<Applicant>? only_applicants = null)
        {
            var consecutive = new AllowedConsecutive()
            {
                Items = { item1, item2 }
            };
            item1.AllowedConsecutives.Add(consecutive);
            item2.AllowedConsecutives.Add(consecutive);
            if (only_applicants != null)
                foreach (var applicant in only_applicants)
                    consecutive.Cast.Add(applicant);
            callbackAfterErrorCorrection(new[] { item1, item2 }, Enumerable.Empty<Role>());
        }

        private List<(string, Action)> NoRoleFixes(Applicant applicant, Section section, IAllocationEngine engine)
        {
            var applicant_name = $"{applicant.FirstName} {applicant.LastName}";
            var items_in_section = section.ItemsInOrder().ToHashSet();
            var available_roles = items_in_section
                .SelectMany(i => i.Roles).Distinct()
                .Where(r => r.RemainingSpacesFor(applicant.CastGroup!, applicant.AlternativeCast) > 0)
                .Where(r => engine.IsEligible(applicant, r))
                .Where(r => engine.IsAvailable(applicant, r));
            var options = new List<(string, Action)>();
            foreach (var role in available_roles)
            {
                var role_items = role.Items.Where(i => items_in_section.Contains(i));
                var option = $"Cast {applicant_name} as {role.Name} in {string.Join(", ", role_items.Select(i => i.Name))}";
                options.Add((option, () => {
                    applicant.Roles.Add(role);
                    role.Cast.Add(applicant);
                    callbackAfterErrorCorrection(Enumerable.Empty<Item>(), role.Yield());
                }));
            }
            if (options.Count == 0)
                options.Add(($"No available roles for {applicant_name}", () => { }));
            return options;
        }

        private List<(string, Action)> MultiRoleFixes(Applicant applicant, Node node)
        {
            var applicant_name = $"{applicant.FirstName} {applicant.LastName}";
            var items_in_node = node.ItemsInOrder().ToHashSet();
            var existing_roles = applicant.Roles
                .Where(r => r.Items.Any(i => items_in_node.Contains(i)));
            var options = new List<(string, Action)>();
            foreach (var role in existing_roles)
            {
                var role_items = role.Items.Where(i => items_in_node.Contains(i));
                var option = $"Remove {applicant_name} from {role.Name} in {string.Join(", ", role_items.Select(i => i.Name))}";
                options.Add((option, () => {
                    applicant.Roles.Remove(role);
                    role.Cast.Remove(applicant);
                    callbackAfterErrorCorrection(Enumerable.Empty<Item>(), role.Yield());
                }));
            }
            return options;
        }
    }
}
