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

        public bool VerifyConsecutiveItems(out List<ConsecutiveItemCast> failures)
            => VerifyConsecutiveItems(out failures, false);

        public bool VerifyConsecutiveItems()
            => VerifyConsecutiveItems(out _, true);

        private bool VerifyConsecutiveItems(out List<ConsecutiveItemCast> failures, bool shortcut_result)
        {
            failures = new();
            if (AllowConsecutiveItems)
                return true; // nothing to check
            var items = ItemsInOrder().ToList();
            if (items.Count < 2)
                return true; // failure is not an option
            //var cast_per_item = items.Select(i => i.Roles.SelectMany(r => r.Cast).ToHashSet()).ToList();
            for (var i = 1; i < items.Count; i++)
            {
                // list roles in each item
                var previous_item_roles = items[i - 1].Roles.ToHashSet();
                var item_roles = items[i].Roles.ToHashSet();
                // separate the roles common to both items
                var legitimate_consecutive_roles = previous_item_roles.Intersect(item_roles).ToHashSet();
                var legitimate_consecutive_cast = legitimate_consecutive_roles.SelectMany(r => r.Cast).ToHashSet();
                previous_item_roles.ExceptWith(legitimate_consecutive_roles);
                item_roles.ExceptWith(legitimate_consecutive_roles);
                // find the set of cast in each item
                var previous_item_cast = previous_item_roles.SelectMany(r => r.Cast).ToHashSet();
                var item_cast = item_roles.SelectMany(r => r.Cast).ToHashSet();
                // check for overlap between item casts
                var cast_in_consecutive_items = previous_item_cast.Intersect(item_cast).ToHashSet();
                // if anyone legitimately cast in a role which is common to both items is ALSO in either item as another role, then they count as consecutive item cast too
                cast_in_consecutive_items.AddRange(legitimate_consecutive_cast.Where(a => previous_item_cast.Contains(a) || item_cast.Contains(a)));
                // remove any allowed consecutive cast
                var allowed_consecutives = items[i - 1].AllowedConsecutives.Intersect(items[i].AllowedConsecutives).ToArray();
                cast_in_consecutive_items.RemoveWhere(a => allowed_consecutives.Any(c => c.IsAllowed(a)));
                // report result
                if (cast_in_consecutive_items.Count != 0)
                {
                    failures.Add(new ConsecutiveItemCast { Item1 = items[i - 1], Item2 = items[i], Cast = cast_in_consecutive_items });
                    if (shortcut_result)
                        return false;
                }
            }
            return failures.Count == 0;
        }
    }
}
