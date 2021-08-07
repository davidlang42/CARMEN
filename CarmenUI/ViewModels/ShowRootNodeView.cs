using ShowModel;
using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class ShowRootNodeView : NodeView
    {
        private ShowRoot showRoot;
        private NodeView[] childrenInOrder;

        public override ICollection<NodeView> ChildrenInOrder => childrenInOrder;

        public override string Name => showRoot.Name;

        public override async Task UpdateAsync()
        {
            StartUpdate();
            var (progress, any_errors) = await UpdateChildren();
            any_errors |= !await Task.Run(() => VerifyConsecutiveItems());
            FinishUpdate(progress, any_errors);
        }

        private bool VerifyConsecutiveItems()//TODO abstract out of RolesSummary.cs, make async
        {
            var items = showRoot.ItemsInOrder().ToList();
            var cast_per_item = items.Select(i => i.Roles.SelectMany(r => r.Cast).ToHashSet()).ToList(); //LATER does this need await? also parallelise
            for (var i = 1; i < items.Count; i++)
            {
                var cast_in_consecutive_items = cast_per_item[i].Intersect(cast_per_item[i - 1]).Count(); //LATER does this need await? also confirm that in-built Intersect() isn't slower than hashset.select(i => other_hashset.contains(i)).count()
                if (cast_in_consecutive_items != 0)
                    return false;
            }
            return true;
        }

        public ShowRootNodeView(ShowRoot show_root, int total_cast)
        {
            showRoot = show_root;
            childrenInOrder = show_root.Children.InOrder().Select(n => CreateView(n, total_cast)).ToArray();
        }
    }
}
