﻿using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
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

namespace Carmen.Desktop.ViewModels
{
    public class RoleOnlyView : RoleView
    {
        private ItemView itemView;

        /// <summary>Indicies match cast_groups provided in constructor</summary>
        public NullableCountByGroup[] CountByGroups { get; init; }

        /// <summary>Indicies match primary_requirements provided in constructor</summary>
        public SelectableObject<Requirement>[] PrimaryRequirements { get; init; }

        public uint TotalCount => CountByGroups.Select(cbg => cbg.Count ?? 0).Sum();

        public string CommaSeparatedItems => string.Join(", ", Role.Items.Select(i => i.Name).OrderBy(n => n));

        public string? CommaSeparatedOtherItems => Role.Items.Count < 2 ? null : string.Join(", ", Role.Items.Where(i => i != itemView.Item).Select(i => i.Name).OrderBy(n => n));

        public RoleOnlyView(Role role, CastGroup[] cast_groups, Requirement[] primary_requirements, ItemView item_view)
            : base(role)
        {
            itemView = item_view;
            CountByGroups = new NullableCountByGroup[cast_groups.Length];
            for (var i = 0; i < cast_groups.Length; i++)
            {
                CountByGroups[i] = new NullableCountByGroup(role.CountByGroups, cast_groups[i]);
                CountByGroups[i].PropertyChanged += CountByGroup_PropertyChanged;
            }
            PrimaryRequirements = new SelectableObject<Requirement>[primary_requirements.Length];
            for (var i = 0; i < primary_requirements.Length; i++)
            {
                PrimaryRequirements[i] = new SelectableObject<Requirement>(role.Requirements, primary_requirements[i]);
                PrimaryRequirements[i].PropertyChanged += PrimaryRequirement_PropertyChanged;
            }
            if (role.Items is ObservableCollection<Item> items)
                items.CollectionChanged += Items_CollectionChanged;
        }

        private void PrimaryRequirement_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(PrimaryRequirements));
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
            foreach (var pr in PrimaryRequirements)
                pr.PropertyChanged -= PrimaryRequirement_PropertyChanged;
            if (Role.Items is ObservableCollection<Item> items)
                items.CollectionChanged -= Items_CollectionChanged;
            base.DisposeInternal();
        }
    }
}
