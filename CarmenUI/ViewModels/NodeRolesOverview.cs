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

namespace CarmenUI.ViewModels
{
    public class NodeRolesOverview
    {
        public Node Node { get; init; }

        public ObservableCollection<IncompleteRole> IncompleteRoles { get; init; } = new();
        public ObservableCollection<string> ValidationErrors { get; init; } = new();

        public NodeRolesOverview(Node node, AlternativeCast[] alternative_casts, IEnumerable<Applicant> cast_members)
        {
            Node = node;
            foreach (var role in node.ItemsInOrder().SelectMany(i => i.Roles).Distinct().InNameOrder())
                if (role.CastingStatus(alternative_casts) != Role.RoleStatus.FullyCast)
                    IncompleteRoles.Add(new IncompleteRole(role));
            if (node is Section section)
            {
                // show section type errors and section consecutive item errors, because they aren't shown anywhere else
                if (!section.CastingMeetsSectionTypeRules(cast_members, out var no_roles, out var multi_roles))
                {
                    foreach (var applicant in no_roles)
                        ValidationErrors.Add($"{FullName.Format(applicant)} has no role in {section.Name}");
                    foreach (var applicant in multi_roles)
                        ValidationErrors.Add($"{FullName.Format(applicant.Key)} has {applicant.Value} roles in {section.Name}");
                }
                if (!section.VerifyConsecutiveItems(out var section_failures))
                    foreach (var failure in section_failures)
                        ValidationErrors.Add($"{failure.Cast.Count.Plural("Applicant is", "Applicants are")} in {failure.Item1.Name} and {failure.Item2.Name}");
            }
            else if (node is Item item)
            {
                // show showroot (and section) consecutive item errors in the item, because showroot isn't visible
                foreach (var consecutive_cast in item.FindConsecutiveCast())
                    foreach (var cast in consecutive_cast.Cast)
                        ValidationErrors.Add($"{cast.FirstName} {cast.LastName} is cast in {consecutive_cast.Item1.Name} and {consecutive_cast.Item2.Name}");
            }
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
