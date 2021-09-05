using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
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

        public NodeRolesOverview(Node node, AlternativeCast[] alternative_casts, uint total_cast_members)
        {
            Node = node;
            foreach (var role in node.ItemsInOrder().SelectMany(i => i.Roles).Distinct().InNameOrder())
                if (role.CastingStatus(alternative_casts) != Role.RoleStatus.FullyCast)
                    IncompleteRoles.Add(new IncompleteRole(role));
            if (node is Section section)
            {
                // show section type errors and section consecutive item errors, because they aren't shown anywhere else
                if (!section.CastingMeetsSectionTypeRules(total_cast_members, out var no_roles, out var multi_roles)) //LATER ideally show *which* applicants dont have roles or have multiple
                {
                    if (no_roles > 0)
                        ValidationErrors.Add($"{no_roles.Plural("Applicant has", "Applicants have")} no role in {section.Name}");
                    if (multi_roles > 0)
                        ValidationErrors.Add($"{multi_roles.Plural("Applicant has", "Applicants have")} multiple roles in {section.Name}");
                }
                if (!section.VerifyConsecutiveItems(out var section_failures))
                    foreach (var failure in section_failures)
                        ValidationErrors.Add($"{failure.CastCount.Plural("Applicant is", "Applicants are")} in {failure.Item1.Name} and {failure.Item2.Name}");
            }
            else if (node is Item item)
            {
                // show showroot (and section) consecutive item errors in the item, because showroot isn't visible
                if (item.Parents().Any(p => !p.AllowConsecutiveItems))
                {
                    var item_roles = item.Roles.ToHashSet();
                    var item_cast = item_roles.SelectMany(r => r.Cast).ToHashSet();
                    if (item.PreviousItem() is Item previous)
                    {
                        var previous_cast = previous.Roles
                            .Where(r => !item_roles.Contains(r)) // a role is allowed to be in 2 consecutive items
                            .SelectMany(r => r.Cast).ToHashSet();
                        previous_cast.IntersectWith(item_cast); // result in previous_cast
                        foreach (var cast in previous_cast)
                            ValidationErrors.Add($"{cast.FirstName} {cast.LastName} is cast in {previous.Name} and {item.Name}");
                    }
                    if (item.NextItem() is Item next)
                    {
                        var next_cast = next.Roles
                            .Where(r => !item_roles.Contains(r)) // a role is allowed to be in 2 consecutive items
                            .SelectMany(r => r.Cast).ToHashSet();
                        next_cast.IntersectWith(item_cast); // result in next_cast
                        foreach (var cast in next_cast)
                            ValidationErrors.Add($"{cast.FirstName} {cast.LastName} is cast in {item.Name} and {next.Name}");
                    }
                }
            }
        }
    }
}
