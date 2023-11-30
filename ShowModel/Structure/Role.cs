using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Requirements;

namespace Carmen.ShowModel.Structure
{
    public class Role : ICounted, INameOrdered, INotifyPropertyChanged
    {
        public enum RoleStatus
        {
            NotCast,
            FullyCast,
            OverCast,
            UnderCast,
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [Key]
        public int RoleId { get; private set; }

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

        private readonly ObservableCollection<CountByGroup> countByGroups = new();
        public virtual ICollection<CountByGroup> CountByGroups => countByGroups;

        private readonly ObservableCollection<Item> items = new();
        public virtual ICollection<Item> Items => items;

        private readonly ObservableCollection<Requirement> requirements = new();
        public virtual ICollection<Requirement> Requirements => requirements;

        private readonly ObservableCollection<Applicant> cast = new();
        public virtual ICollection<Applicant> Cast => cast;

        public Role()
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

        public uint CountFor(CastGroup group)
            => CountByGroups.Where(c => c.CastGroup.CastGroupId == group.CastGroupId).Select(c => c.Count).SingleOrDefault(); // defaults to 0

        public long RemainingSpacesFor(CastGroup group, AlternativeCast? alterative_cast)
            => CountFor(group) - Cast.Where(a => a.CastGroup == group && a.AlternativeCast == alterative_cast).Count();

        public RoleStatus CastingStatus(AlternativeCast[] alternative_casts)
        {
            if (Cast.Count == 0)
                return RoleStatus.NotCast;
            var required_cast_groups = CountByGroups.Where(cbg => cbg.Count != 0).Select(cbg => cbg.CastGroup).ToHashSet();
            if (required_cast_groups.Count == 0)
                return RoleStatus.OverCast;
            bool under_cast = false;
            foreach (var cast_by_group in Cast.GroupBy(a => a.CastGroup))
            {
                if (cast_by_group.Key is not CastGroup cast_group)
                    return RoleStatus.OverCast;
                if (!required_cast_groups.Remove(cast_group)) // if this cast_group isn't required
                    return RoleStatus.OverCast;
                var group_casts = cast_group.AlternateCasts ? alternative_casts : new AlternativeCast?[] { null };
                var required_count = CountFor(cast_group);
                foreach (var alternative_cast in group_casts)
                {
                    var actual_count = cast_by_group.Where(a => a.AlternativeCast?.AlternativeCastId == alternative_cast?.AlternativeCastId).Count();
                    if (actual_count > required_count)
                        return RoleStatus.OverCast;
                    if (actual_count < required_count)
                        under_cast = true; // don't return yet, because if any count is over cast, we want to return that first
                }
            }
            if (under_cast || required_cast_groups.Count > 0)
                return RoleStatus.UnderCast;
            return RoleStatus.FullyCast;
        }

        /// <summary>Returns a set of all the items and sections which contain this Role</summary>
        public HashSet<Node> ItemsAndSections()
            => Items.Cast<Node>().Concat(Items.SelectMany(item => item.Parents())).Where(n => n is not ShowRoot).ToHashSet();

        public static HashSet<Node> CommonItemsAndSections(params Role[] roles)
        {
            if (roles.Length == 0)
                return new HashSet<Node>();
            var parents = roles[0].ItemsAndSections();
            for (var i = 1; i < roles.Length; i++)
                parents.IntersectWith(roles[i].ItemsAndSections());
            return parents;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
