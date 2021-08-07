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
            //TODO update show root
            await Task.Run(() => Thread.Sleep(1000));
            FinishUpdate(0.75, false);
        }

        public ShowRootNodeView(ShowRoot show_root)
        {
            showRoot = show_root;
            childrenInOrder = show_root.Children.InOrder().Select(n => CreateView(n)).ToArray();
        }
    }
}
