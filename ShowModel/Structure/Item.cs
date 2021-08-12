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
            // Only propogate changes to counts
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(CountByGroups))
                OnPropertyChanged(nameof(CountByGroups));
        }

        public override IEnumerable<Item> ItemsInOrder() => this.Yield();

        public Item? NextItem()
        {
            var e = RootParent().ItemsInOrder().GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current == this)
                    return e.MoveNext() ? e.Current : null;
            }
            throw new ApplicationException("Item not found in root parent's items.");
        }

        public Item? PreviousItem()
        {
            var e = RootParent().ItemsInOrder().GetEnumerator();
            Item? last_item = null;
            while (e.MoveNext())
            {
                if (e.Current == this)
                    return last_item;
                last_item = e.Current;
            }
            throw new ApplicationException("Item not found in root parent's items.");
        }
    }
}
