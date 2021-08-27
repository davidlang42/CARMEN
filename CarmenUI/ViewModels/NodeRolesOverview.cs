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

        public NodeRolesOverview(Node node, AlternativeCast[] alternative_casts)
        {
            Node = node;
            foreach (var role in node.ItemsInOrder().SelectMany(i => i.Roles).Distinct().InOrder())
                if (role.CastingStatus(alternative_casts) != Role.RoleStatus.FullyCast)
                    IncompleteRoles.Add(new IncompleteRole(role));
            //TODO (BALANCE) add real validation errors
            //- show errors of the current node, with similar but not identical wording to mainmenu summary
            //- only show validation issues which aren't shown anywhere else (eg. in the role selection itself, or the next item down)
            //  -- OR is it better just to show them?
            for (var i = 0; i < 15; i++)
                ValidationErrors.Add($"Validation error {i + 1}");
        }
    }
}
