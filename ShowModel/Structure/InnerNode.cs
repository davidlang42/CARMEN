﻿using System;
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
            children.CollectionChanged += Children_CollectionChanged; //TODO dispose handlers
        }

        private void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Children));
            if (e.Action != NotifyCollectionChangedAction.Move)
            {
                var new_items = e.NewItems?.Cast<Node>().ToHashSet() ?? new HashSet<Node>();
                var old_items = e.OldItems?.Cast<Node>().ToHashSet() ?? new HashSet<Node>();
                foreach (var added in new_items.Where(n => !old_items.Contains(n)))
                    added.PropertyChanged += Child_PropertyChanged; //TODO dispose handlers
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
            var cast_per_item = items.Select(i => i.Roles.SelectMany(r => r.Cast).ToHashSet()).ToList();
            for (var i = 1; i < items.Count; i++)
            {
                var cast_in_consecutive_items = cast_per_item[i].Intersect(cast_per_item[i - 1]).ToHashSet();
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
