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
            any_errors |= !await Task.Run(() => VerifySectionTypeRules());
            FinishUpdate(progress, any_errors);
        }

        private bool VerifySectionTypeRules() //TODO abstract out of RolesSummary.cs, make async
        {
            var section_type = section.SectionType;
            if (section_type.AllowNoRoles && section_type.AllowMultipleRoles)
                return true; // nothing to check
            var roles_in_section = section.ItemsInOrder().SelectMany(i => i.Roles).ToHashSet(); //LATER does this need await?
            if (!section_type.AllowNoRoles)
            {
                var no_roles = totalCast - roles_in_section.SelectMany(r => r.Cast).Distinct().Count(); //LATER does this need await?
                if (no_roles > 0)
                    return false;
            }
            if (!section_type.AllowMultipleRoles)
            {
                var multi_roles = roles_in_section.SelectMany(r => r.Cast).GroupBy(a => a).Where(g => g.Count() > 1).Count(); //LATER does this need await?
                if (multi_roles > 0)
                    return false;
            }
            return true;
        }

        public SectionNodeView(Section section, int total_cast)
        {
            this.section = section;
            this.totalCast = total_cast;
            childrenInOrder = section.Children.InOrder().Select(n => CreateView(n, total_cast)).ToArray();
        }
    }
}
