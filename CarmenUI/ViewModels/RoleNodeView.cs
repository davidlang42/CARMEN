using ShowModel.Applicants;
using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class RoleNodeView : NodeView
    {
        public Role Role;
        private NodeView[] childrenInOrder;
        private AlternativeCast[] alternativeCasts;

        public override ICollection<NodeView> ChildrenInOrder => childrenInOrder;

        public override string Name => Role.Name;

        public override async Task UpdateAsync()
        {
            StartUpdate();
            // check role status
            var status = await Task.Run(() => Role.CastingStatus(alternativeCasts));
            var is_complete = status == Role.RoleStatus.FullyCast;
            var has_errors = status == Role.RoleStatus.UnderCast || status == Role.RoleStatus.OverCast;
            FinishUpdate(is_complete ? 1 : 0, has_errors);
        }

        public RoleNodeView(Role role, AlternativeCast[] alternative_casts)
        {
            this.Role = role;
            childrenInOrder = new NodeView[0];
            alternativeCasts = alternative_casts;
        }
    }
}
