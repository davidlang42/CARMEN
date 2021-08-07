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
        private Role role;
        private NodeView[] childrenInOrder;

        public override ICollection<NodeView> ChildrenInOrder => childrenInOrder;

        public override string Name => role.Name;

        public override async Task UpdateAsync()
        {
            StartUpdate();
            //TODO update role
            await Task.Run(() => Thread.Sleep(5000));
            FinishUpdate(1, false);
        }

        public RoleNodeView(Role role)
        {
            this.role = role;
            childrenInOrder = new NodeView[0];
        }
    }
}
