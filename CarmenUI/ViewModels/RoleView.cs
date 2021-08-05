using ShowModel.Applicants;
using ShowModel.Requirements;
using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class RoleView : IDisposable, INotifyPropertyChanged
    {
        private Role role;
        private CountByGroup[]? countByGroups;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => role.Name;
            set
            {
                if (role.Name == value)
                    return;
                role.Name = value;
                OnPropertyChanged();
            }
        }

        public CountByGroup[] CountByGroups => countByGroups ?? throw new ApplicationException("Tried to use RoleView after it was disposed.");

        public uint TotalCount => CountByGroups.Select(cbg => cbg.Count).Sum();

        public ICollection<Requirement> Requirements => role.Requirements;

        //TODO implement RequirementsSummary here rather than use nameLister

        public RoleView(Role role, CastGroup[] cast_groups)
        {
            this.role = role;
            this.countByGroups = new CountByGroup[cast_groups.Length];
            for (var i = 0; i < cast_groups.Length; i++)
            {
                if (role.CountByGroups.SingleOrDefault(cbg => cbg.CastGroup == cast_groups[i]) is not CountByGroup cbg)
                {
                    cbg = new CountByGroup { CastGroup = cast_groups[i], Count = 0 };
                    role.CountByGroups.Add(cbg);
                }
                countByGroups[i] = cbg;
                cbg.PropertyChanged += CountByGroup_PropertyChanged;
            }
        }

        private void CountByGroup_PropertyChanged(object? sender, PropertyChangedEventArgs e)
            => OnPropertyChanged(nameof(TotalCount));

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void Dispose()
        {
            if (countByGroups != null)
            {
                foreach (var cbg in countByGroups)
                    cbg.PropertyChanged -= CountByGroup_PropertyChanged;
                countByGroups = null;
            }
        }
    }
}
