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
        public CountByGroup[] CountByGroups { get; init; }

        public uint TotalCount => CountByGroups.Select(cbg => cbg.Count).Sum();

        public RoleOnlyView(Role role, CastGroup[] cast_groups)
            : base(role)
        {
            CastGroups = cast_groups;
            CountByGroups = new CountByGroup[cast_groups.Length];
            for (var i = 0; i < cast_groups.Length; i++)
            {
                if (role.CountByGroups.SingleOrDefault(cbg => cbg.CastGroup == cast_groups[i]) is not CountByGroup cbg)
                {
                    cbg = new CountByGroup { CastGroup = cast_groups[i], Count = 0 };
                    role.CountByGroups.Add(cbg);//LATER (maybe) instead of creating missing CountByGroups, they could be wrapped as NullableCountByGroups, which might give a better UX, as long as it doesn't break anything else
                }
                CountByGroups[i] = cbg;
                cbg.PropertyChanged += CountByGroup_PropertyChanged;
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
