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
    public class ItemNodeView : NodeView
    {
        private Item item;
        private NodeView[] childrenInOrder;

        public override ICollection<NodeView> ChildrenInOrder => childrenInOrder;

        public override string Name => item.Name;

        public override async Task UpdateAsync()
        {
            StartUpdate();
            var (progress, any_errors) = await UpdateChildren();
            FinishUpdate(progress, any_errors);
        }

        public ItemNodeView(Item item, AlternativeCast[] alternative_casts)
        {
            this.item = item;
            childrenInOrder = item.Roles.OrderBy(r => r.Name).Select(r => new RoleNodeView(r, alternative_casts)).ToArray(); //LATER might need to handle observable collections here so that if roles are added, it gets picked up
        }
    }
}
