﻿using ShowModel;
using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class SectionNodeView : NodeView
    {
        private Section section;
        private NodeView[] childrenInOrder;

        public override ICollection<NodeView> ChildrenInOrder => childrenInOrder;

        public override string Name => section.Name;

        public override async Task UpdateAsync()
        {
            StartUpdate();
            //TODO update section
            await Task.Run(() => Thread.Sleep(2000));
            FinishUpdate(0.5, true);
        }

        public SectionNodeView(Section section)
        {
            this.section = section;
            childrenInOrder = section.Children.InOrder().Select(n => CreateView(n)).ToArray();
        }
    }
}
