using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using CarmenUI.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CarmenUI.ViewModels
{
    public class NodeRolesOverview
    {
        Action<IEnumerable<Item>, IEnumerable<Role>> callbackAfterErrorCorrection;

        public Node Node { get; }

        public ObservableCollection<IncompleteRole> IncompleteRoles { get; }
        public ObservableCollection<CastingError> CastingErrors { get; }

        /// <summary>Node should be an Item or Section, NOT ShowRoot</summary>
        public NodeRolesOverview(Node node, AlternativeCast[] alternative_casts, IEnumerable<Applicant> cast_members, Action<IEnumerable<Item>, IEnumerable<Role>> callback_after_error_correction)
        {
            callbackAfterErrorCorrection = callback_after_error_correction;
            Node = node;
            IncompleteRoles = FindIncompleteRoles(node, alternative_casts).ToObservableCollection();
            if (node is Section section)
                CastingErrors = FindSectionCastingErrors(section, cast_members).ToObservableCollection();
            else if (node is Item item)
                CastingErrors = FindItemCastingErrors(item).ToObservableCollection();
            else
                throw new NotImplementedException($"Node type {node.GetType().Name} not handled.");
        }

        private static IEnumerable<IncompleteRole> FindIncompleteRoles(Node node, AlternativeCast[] alternative_casts)
        {
            foreach (var role in node.ItemsInOrder().SelectMany(i => i.Roles).Distinct().InNameOrder())
                if (role.CastingStatus(alternative_casts) != Role.RoleStatus.FullyCast)
                    yield return new IncompleteRole(role);
        }

        /// <summary>Show section type errors and section consecutive item errors (because they aren't shown anywhere else)</summary>
        private IEnumerable<CastingError> FindSectionCastingErrors(Section section, IEnumerable<Applicant> cast_members)
        {
            if (!section.CastingMeetsSectionTypeRules(cast_members, out var no_roles, out var multi_roles))
            {
                foreach (var applicant in no_roles)
                    yield return new CastingError($"{FullName.Format(applicant)} has no role in {section.Name}");
                foreach (var applicant in multi_roles)
                    yield return new CastingError($"{FullName.Format(applicant.Key)} has {applicant.Value} roles in {section.Name}");
            }
            if (!section.VerifyConsecutiveItems(out var section_failures))
                foreach (var failure in section_failures)
                    yield return new CastingError($"{failure.Cast.Count.Plural("applicant is", "applicants are")} in {failure.Item1.Name} and {failure.Item2.Name}", right_click: new()
                    {
                        { $"Allow {(failure.Cast.Count == 1 ? "this" : "these")} consecutive cast only", () => AllowConsecutive(failure.Item1, failure.Item2, failure.Cast) },
                        { "Allow all consecutive cast between these items", () => AllowConsecutive(failure.Item1, failure.Item2) }
                    });
        }

        /// <summary>Show cast with multiple roles in this item and showroot (and section) consecutive item errors in the item (because showroot isn't visible)</summary>
        private IEnumerable<CastingError> FindItemCastingErrors(Item item)
        {
            foreach (var applicant in item.FindDuplicateCast())
                yield return new CastingError($"{FullName.Format(applicant.Key)} has {applicant.Value} roles in {item.Name}");
            foreach (var consecutive_cast in item.FindConsecutiveCast())
                foreach (var cast in consecutive_cast.Cast)
                    yield return new CastingError($"{FullName.Format(cast)} is cast in {consecutive_cast.Item1.Name} and {consecutive_cast.Item2.Name}", right_click: new()
                    {
                        { "Allow this consecutive cast only", () => AllowConsecutive(consecutive_cast.Item1, consecutive_cast.Item2, cast.Yield()) },
                        { "Allow all consecutive cast between these items", () => AllowConsecutive(consecutive_cast.Item1, consecutive_cast.Item2) }
                    });
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

        /// <summary>Find the given roles in the IncompleteRoles list, and mark them as selected.
        /// Ignore any roles which are not found. Does not deselect other roles.</summary>
        public void SelectRoles(IEnumerable<Role> roles)
        {
            var roles_to_select = roles.ToHashSet();
            foreach (var incomplete_role in IncompleteRoles)
                if (roles_to_select.Contains(incomplete_role.Role))
                    incomplete_role.IsSelected = true;
        }
    }
}
