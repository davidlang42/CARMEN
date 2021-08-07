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
            any_errors |= !await Task.Run(() => showRoot.VerifyConsecutiveItems());
            FinishUpdate(progress, any_errors);
        }

        public ShowRootNodeView(ShowRoot show_root, int total_cast)
        {
            showRoot = show_root;
            childrenInOrder = show_root.Children.InOrder().Select(n => CreateView(n, total_cast)).ToArray();
        }
    }
}
