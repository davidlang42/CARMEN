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
    public class RoleView : INotifyPropertyChanged//TODO do we really need this? Could I just use a converter? prob not
    {
        private Role role;
        private CastGroup[] castGroups;

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

        public CountByGroup[] CountByGroups//TODO really this should be made on construction, then we know exactly which 4 CBG objects we need to monitor for changes
        {
            get
            {
                var count_by_groups = new CountByGroup[castGroups.Length];
                for (var i=0; i< castGroups.Length; i++)
                {
                    if (role.CountByGroups.SingleOrDefault(cbg => cbg.CastGroup == castGroups[i]) is not CountByGroup cbg)
                    {
                        cbg = new CountByGroup { CastGroup = castGroups[i], Count = 0 };
                        role.CountByGroups.Add(cbg);
                    }
                    count_by_groups[i] = cbg;
                }
                return count_by_groups;
            }
        }

        public uint TotalCount => role.CountByGroups.Select(cbg => cbg.Count).Sum();

        public ICollection<Requirement> Requirements => role.Requirements;

        public RoleView(Role role, CastGroup[] cast_groups)
        {
            this.role = role;
            this.castGroups = cast_groups;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
