using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Structure
{
    /// <summary>
    /// An internal node of the item tree, which can have children.
    /// </summary>
    public abstract class InnerNode : Node
    {
        private readonly ObservableCollection<Node> children = new();
        public virtual ICollection<Node> Children => children;

        public override IEnumerable<Item> ItemsInOrder() => Children.InOrder().SelectMany(n => n.ItemsInOrder());

        protected abstract bool _allowConsecutiveItems { get; }

        public bool AllowConsecutiveItems => _allowConsecutiveItems;

        public InnerNode()
        {
            children.CollectionChanged += Children_CollectionChanged;
        }

        private void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Children));
            if (e.Action != NotifyCollectionChangedAction.Move)
            {
                var new_items = e.NewItems?.Cast<Node>().ToHashSet() ?? new HashSet<Node>();
                var old_items = e.OldItems?.Cast<Node>().ToHashSet() ?? new HashSet<Node>();
                foreach (var added in new_items.Where(n => !old_items.Contains(n)))
                    added.PropertyChanged += Child_PropertyChanged;
                foreach (var removed in old_items.Where(o => !new_items.Contains(o)))
                    removed.PropertyChanged -= Child_PropertyChanged;
            }
        }

        private void Child_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Only propogate changes to counts
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(CountByGroups))
                OnPropertyChanged(nameof(CountByGroups));
        }

        public bool VerifyConsecutiveItems(out List<ConsecutiveItemSummary> failures)
            => VerifyConsecutiveItems(out failures, false);

        public bool VerifyConsecutiveItems()
            => VerifyConsecutiveItems(out _, true);

        private bool VerifyConsecutiveItems(out List<ConsecutiveItemSummary> failures, bool shortcut_result)
        {
            failures = new();
            if (AllowConsecutiveItems)
                return true; // nothing to check
            var items = ItemsInOrder().ToList();
            //LATER possible bug: what happens if a role is in 2 consecutive items?
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
