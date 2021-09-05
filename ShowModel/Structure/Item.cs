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
    }
}
