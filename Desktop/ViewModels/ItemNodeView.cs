using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Carmen.Desktop.ViewModels
{
    public class ItemNodeView : NodeView
    {
        public Item Item { get; init; }

        public override string Name => Item.Name;

        protected override async Task<(double progress, bool has_errors)> CalculateAsync(double child_progress, bool child_errors)
        {
            child_errors |= await Task.Run(() => Item.FindDuplicateCast().Any());
            child_errors |= await Task.Run(() => Item.FindConsecutiveCast().Any());
            return (child_progress, child_errors);
        }

        public ItemNodeView(Item item, AlternativeCast[] alternative_casts)
            : base(item.Roles.OrderBy(r => r.Name).Select(r => new RoleNodeView(r, alternative_casts)))
        {
            this.Item = item;
        }
    }
}
