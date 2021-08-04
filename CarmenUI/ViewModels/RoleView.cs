using ShowModel.Applicants;
using ShowModel.Requirements;
using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class RoleView : INotifyPropertyChanged
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

        public uint?[] Count
        {
            get => castGroups.Select(cg => role.CountByGroups.Where(cbg => cbg.CastGroup == cg).SingleOrDefault()?.Count).ToArray();
            set
            {
                if (value.Length != castGroups.Length)
                    throw new ArgumentException("New array length did not match existing.");
                bool changes = false;
                for (var i = 0; i < castGroups.Length; i++)
                {
                    var cbg = role.CountByGroups.Where(cbg => cbg.CastGroup == castGroups[i]).SingleOrDefault();
                    if (value[i] != cbg?.Count) {
                        if (cbg == null)
                        {
                            cbg = new CountByGroup { CastGroup = castGroups[i], Count = value[i]!.Value };
                            role.CountByGroups.Add(cbg);
                        }
                        else if (value[i] == null)
                            role.CountByGroups.Remove(cbg);
                        else
                            cbg.Count = value[i]!.Value;
                        changes = true;
                    }
                }
                if (changes)
                {
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalCount));
                }
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
