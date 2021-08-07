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
        private int totalCast;
        private NodeView[] childrenInOrder;

        public override ICollection<NodeView> ChildrenInOrder => childrenInOrder;

        public override string Name => section.Name;

        public override async Task UpdateAsync()
        {
            StartUpdate();
            // check child statuses
            var (progress, any_errors) = await UpdateChildren();
            // check section type rules
            any_errors |= !await Task.Run(() => section.CastingMeetsSectionTypeRules(totalCast, out _, out _));
            FinishUpdate(progress, any_errors);
        }

        public SectionNodeView(Section section, int total_cast)
        {
            this.section = section;
            this.totalCast = total_cast;
            childrenInOrder = section.Children.InOrder().Select(n => CreateView(n, total_cast)).ToArray();
        }
    }
}
