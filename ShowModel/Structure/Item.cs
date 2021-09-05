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
    }
}
