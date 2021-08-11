using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class RoleOnlyView : RoleView
    {
        public CastGroup[] CastGroups { get; init; }

        /// <summary>Indicies match CastGroups</summary>
        public NullableCountByGroup[] CountByGroups { get; init; }

        public uint TotalCount => CountByGroups.Select(cbg => cbg.Count ?? 0).Sum();

        public RoleOnlyView(Role role, CastGroup[] cast_groups)
            : base(role)
        {
            CastGroups = cast_groups;
            CountByGroups = new NullableCountByGroup[cast_groups.Length];
            for (var i = 0; i < cast_groups.Length; i++)
            {
                CountByGroups[i] = new NullableCountByGroup(role.CountByGroups, cast_groups[i]);
                CountByGroups[i].PropertyChanged += CountByGroup_PropertyChanged;
            }
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
            base.DisposeInternal();
        }
    }
}
