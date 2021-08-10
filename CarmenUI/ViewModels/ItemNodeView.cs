using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
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

        public override string Name => item.Name;

        protected override Task<(double progress, bool has_errors)> CalculateAsync(double child_progress, bool child_errors)
            => Task.Run(() => (child_progress, child_errors));

        public ItemNodeView(Item item, AlternativeCast[] alternative_casts)
            : base(item.Roles.OrderBy(r => r.Name).Select(r => new RoleNodeView(r, alternative_casts)))
        {
            this.item = item;
        }
    }
}
