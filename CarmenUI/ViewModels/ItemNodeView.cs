using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class ItemNodeView : NodeView
    {
        private Item item;
        private NodeView[] childrenInOrder;

        public override ICollection<NodeView> ChildrenInOrder => childrenInOrder;

        public override string Name => item.Name;

        public ItemNodeView(Item item)
        {
            this.item = item;
            childrenInOrder = item.Roles.OrderBy(r => r.Name).Select(r => new RoleNodeView(r)).ToArray(); //LATER might need to handle observable collections here so that if roles are added, it gets picked up
        }
    }
}
