using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Structure
{
    /// <summary>
    /// An internal node of the item tree, which can have children.
    /// </summary>
    public abstract class InnerNode : Node //LATER implement INotifyPropertyChanged for completeness
    {
        public virtual ICollection<Node> Children { get; private set; } = new ObservableCollection<Node>();

        public override IEnumerable<Item> ItemsInOrder() => Children.InOrder().SelectMany(n => n.ItemsInOrder());

        protected abstract bool GetAllowConsecutiveItems();

        public bool VerifyConsecutiveItems(out List<ConsecutiveItemSummary> failures)
            => VerifyConsecutiveItems(out failures, false);

        public bool VerifyConsecutiveItems()
            => VerifyConsecutiveItems(out _, true);

        private bool VerifyConsecutiveItems(out List<ConsecutiveItemSummary> failures, bool shortcut_result)
        {
            failures = new();
            if (GetAllowConsecutiveItems())
                return true; // nothing to check
            var items = ItemsInOrder().ToList();
            var cast_per_item = items.Select(i => i.Roles.SelectMany(r => r.Cast).ToHashSet()).ToList(); //LATER does this need await? also parallelise
            for (var i = 1; i < items.Count; i++)
            {
                var cast_in_consecutive_items = cast_per_item[i].Intersect(cast_per_item[i - 1]).Count(); //LATER does this need await? also confirm that in-built Intersect() isn't slower than hashset.select(i => other_hashset.contains(i)).count()
                if (cast_in_consecutive_items != 0)
                {
                    failures.Add(new ConsecutiveItemSummary { Item1 = items[i - 1], Item2 = items[i], CastCount = cast_in_consecutive_items });
                    if (shortcut_result)
                        return false;
                }
            }
            return failures.Count == 0;
        }
    }

    public struct ConsecutiveItemSummary
    {
        public Item Item1;
        public Item Item2;
        public int CastCount;
    }
}
