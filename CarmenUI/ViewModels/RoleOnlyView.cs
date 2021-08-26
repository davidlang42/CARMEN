using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class RoleOnlyView : RoleView
    {
        private ItemView itemView;

        public CastGroup[] CastGroups { get; init; }

        /// <summary>Indicies match CastGroups</summary>
        public NullableCountByGroup[] CountByGroups { get; init; }

        public uint TotalCount => CountByGroups.Select(cbg => cbg.Count ?? 0).Sum();

        public ICollection<Item> Items => Role.Items;

        public string CommaSeparatedItems => string.Join(", ", Role.Items.Select(i => i.Name).OrderBy(n => n));

        public string? CommaSeparatedOtherItems => Role.Items.Count < 2 ? null : string.Join(", ", Role.Items.Where(i => i != itemView.Item).Select(i => i.Name).OrderBy(n => n));

        public RoleOnlyView(Role role, CastGroup[] cast_groups, ItemView item_view)
            : base(role)
        {
            itemView = item_view;
            CastGroups = cast_groups;
            CountByGroups = new NullableCountByGroup[cast_groups.Length];
            for (var i = 0; i < cast_groups.Length; i++)
            {
                CountByGroups[i] = new NullableCountByGroup(role.CountByGroups, cast_groups[i]);
                CountByGroups[i].PropertyChanged += CountByGroup_PropertyChanged;
            }
            if (role.Items is ObservableCollection<Item> items)
                items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CommaSeparatedItems));
            OnPropertyChanged(nameof(CommaSeparatedOtherItems));
            if (e.Action != NotifyCollectionChangedAction.Move
                && e.OldItems is IList removed_items
                && removed_items.Contains(itemView.Item))
                itemView.RemoveRole(this);
        }

        private void CountByGroup_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CountByGroups));
            OnPropertyChanged(nameof(TotalCount));
        }

        protected override void DisposeInternal()
        {
            foreach (var cbg in CountByGroups)
                cbg.PropertyChanged -= CountByGroup_PropertyChanged;
            if (Role.Items is ObservableCollection<Item> items)
                items.CollectionChanged -= Items_CollectionChanged;
            base.DisposeInternal();
        }
    }
}
