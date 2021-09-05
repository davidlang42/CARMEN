using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Carmen.ShowModel.Structure
{
    /// <summary>
    /// An item within the show.
    /// </summary>
    public class Item : Node
    {
        private readonly ObservableCollection<Role> roles = new();
        public virtual ICollection<Role> Roles => roles;

        public Item()
        {
            roles.CollectionChanged += Roles_CollectionChanged;
        }

        private void Roles_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Roles));
            if (e.Action != NotifyCollectionChangedAction.Move)
            {
                var new_items = e.NewItems?.Cast<Role>().ToHashSet() ?? new HashSet<Role>();
                var old_items = e.OldItems?.Cast<Role>().ToHashSet() ?? new HashSet<Role>();
                foreach (var added in new_items.Where(n => !old_items.Contains(n)))
                    added.PropertyChanged += Role_PropertyChanged;
                foreach (var removed in old_items.Where(o => !new_items.Contains(o)))
                    removed.PropertyChanged -= Role_PropertyChanged;
            }
        }

        private void Role_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Only propogate changes to counts and name
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(Role.CountByGroups))
                OnPropertyChanged(nameof(CountByGroups));
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(Role.Name))
                OnPropertyChanged(nameof(Roles ));
        }

        public override IEnumerable<Item> ItemsInOrder() => this.Yield();

        public Item? NextItem()
        {
            Node node = this;
            while (node is not ShowRoot)
            {
                var below = node.SiblingBelow();
                if (below is Item item_below)
                    return item_below;
                else if (below is InnerNode inner_below)
                {
                    if (inner_below.ItemsInOrder().FirstOrDefault() is Item first_of_inner_below)
                        return first_of_inner_below;
                    else
                        node = inner_below;
                }
                else if (below == null)
                    node = node.Parent ?? throw new ApplicationException("Non-ShowRoot must have parent.");
                else
                    throw new ApplicationException($"Node type not handled: {node.GetType().Name}");
            }
            return null; // if no sibling below and parent is showroot, we are really the bottom
        }

        public Item? PreviousItem()
        {
            Node node = this;
            while (node is not ShowRoot)
            {
                var above = node.SiblingAbove();
                if (above is Item item_above)
                    return item_above;
                else if (above is InnerNode inner_above)
                {
                    if (inner_above.ItemsInOrder().LastOrDefault() is Item last_of_inner_above)
                        return last_of_inner_above;
                    else
                        node = inner_above;
                }
                else if (above == null)
                    node = node.Parent ?? throw new ApplicationException("Non-ShowRoot must have parent.");
                else
                    throw new ApplicationException($"Node type not handled: {node.GetType().Name}");
            }
            return null; // if no sibling below and parent is showroot, we are really the bottom
        }

        /// <summary>Find cast allocated to roles consecutive items (previous item, this item) or (this item, next item).
        /// NOTE: If all parent Sections and ShowRoot allow consecutive items, this will return an empty sequence.</summary>
        public IEnumerable<ConsecutiveItemCast> FindConsecutiveCast()
        {
            var item_roles = Roles.ToHashSet();
            var item_cast = item_roles.SelectMany(r => r.Cast).ToHashSet();
            if (PreviousItem() is Item previous && CommonParents(previous, this).Any(p => !p.AllowConsecutiveItems))
            {
                var previous_cast = previous.Roles
                    .Where(r => !item_roles.Contains(r)) // a role is allowed to be in 2 consecutive items
                    .SelectMany(r => r.Cast).ToHashSet();
                previous_cast.IntersectWith(item_cast); // result in previous_cast
                yield return new ConsecutiveItemCast { Cast = previous_cast, Item1 = previous, Item2 = this };
            }
            if (NextItem() is Item next && CommonParents(this, next).Any(p => !p.AllowConsecutiveItems))
            {
                var next_cast = next.Roles
                    .Where(r => !item_roles.Contains(r)) // a role is allowed to be in 2 consecutive items
                    .SelectMany(r => r.Cast).ToHashSet();
                next_cast.IntersectWith(item_cast); // result in next_cast
                yield return new ConsecutiveItemCast { Cast = next_cast, Item1 = this, Item2 = next };
            }
        }

        public static HashSet<InnerNode> CommonParents(params Item[] items)
        {
            if (items.Length == 0)
                return new HashSet<InnerNode>();
            var parents = items[0].Parents().ToHashSet();
            for (var i = 1; i < items.Length; i++)
                parents.IntersectWith(items[i].Parents());
            return parents;
        }
    }
}
