using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using Carmen.ShowModel.Applicants;

namespace Carmen.ShowModel.Structure
{
    /// <summary>
    /// A node in the item tree, which may or may not be able to have children.
    /// </summary>
    public abstract class Node : IOrdered, ICounted, INamed, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [Key]
        public int NodeId { get; private set; }

        private string name = "";
        public string Name
        {
            get => name;
            set
            {
                if (name == value)
                    return;
                name = value;
                OnPropertyChanged();
            }
        }

        private int order;
        public int Order
        {
            get => order;
            set
            {
                if (order == value)
                    return;
                order = value;
                OnPropertyChanged();
            }
        }

        private InnerNode? parent;
        public virtual InnerNode? Parent
        {
            get => parent;
            set
            {
                if (parent == value)
                    return;
                parent = value;
                OnPropertyChanged();
            }
        }

        private readonly ObservableCollection<CountByGroup> countByGroups = new();
        public virtual ICollection<CountByGroup> CountByGroups => countByGroups;

        public Node()
        {
            countByGroups.CollectionChanged += CountByGroups_CollectionChanged;
        }

        private void CountByGroups_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CountByGroups));
            if (e.Action != NotifyCollectionChangedAction.Move)
            {
                var new_items = e.NewItems?.Cast<CountByGroup>().ToHashSet() ?? new HashSet<CountByGroup>();
                var old_items = e.OldItems?.Cast<CountByGroup>().ToHashSet() ?? new HashSet<CountByGroup>();
                foreach (var added in new_items.Where(n => !old_items.Contains(n)))
                    added.PropertyChanged += CountByGroup_PropertyChanged;
                foreach (var removed in old_items.Where(o => !new_items.Contains(o)))
                    removed.PropertyChanged -= CountByGroup_PropertyChanged;
            }
        }

        private void CountByGroup_PropertyChanged(object? sender, PropertyChangedEventArgs e)
            => OnPropertyChanged(nameof(CountByGroups));

        public abstract IEnumerable<Item> ItemsInOrder();

        public uint CountFor(CastGroup group)
            => CountByGroups.Where(c => c.CastGroup == group).SingleOrDefault()?.Count
            ?? ItemsInOrder().SelectMany(i => i.Roles).Distinct().Select(r => r.CountFor(group)).Sum();

        /// <summary>Recursively enumerate all parents</summary>
        public IEnumerable<InnerNode> Parents()
        {
            var parent = Parent;
            while (parent != null)
            {
                yield return parent;
                parent = parent.Parent;
            }
        }

        /// <summary>Find the sibling directly above this Node, or null if top of parent</summary>
        public Node? SiblingAbove()
        {
            if (parent == null)
                throw new ApplicationException("Cannot get siblings when parent is null.");
            Node? last = null;
            foreach(var sibling in parent.Children.InOrder())
            {
                if (sibling == this)
                    return last;
                last = sibling;
            }
            throw new ApplicationException("Node not found in parent.");
        }

        /// <summary>Find the sibling directly below this Node, or null if bottom of parent</summary>
        public Node? SiblingBelow()
        {
            if (parent == null)
                throw new ApplicationException("Cannot get siblings when parent is null.");
            var e = parent.Children.InOrder().GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current == this)
                {
                    if (e.MoveNext())
                        return e.Current;
                    else
                        return null;
                }
            }
            throw new ApplicationException("Node not found in parent.");
        }

        public bool CountMatchesSumOfRoles()
        {
            if (CountByGroups.Count == 0)
                return true;
            var remaining_counts = CountByGroups.ToDictionary(cbg => cbg.CastGroup, cbg => (int)cbg.Count);
            foreach (var role in ItemsInOrder().SelectMany(i => i.Roles).Distinct())
                foreach (var role_cbg in role.CountByGroups)
                    if (remaining_counts.TryGetValue(role_cbg.CastGroup, out var old_remaining))
                    {
                        var new_remaining = old_remaining - (int)role_cbg.Count;
                        if (new_remaining < 0)
                            return false;
                        remaining_counts[role_cbg.CastGroup] = new_remaining;
                    }
            if (remaining_counts.Values.Any(r => r != 0))
                return false;
            return true;
        }

        protected Dictionary<Applicant, int> CountRolesPerCastMember() => ItemsInOrder()
            .SelectMany(i => i.Roles).Distinct()
            .SelectMany(r => r.Cast).GroupBy(a => a)
            .ToDictionary(g => g.Key, g => g.Count());

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
