using ShowModel.Applicants;
using ShowModel.Requirements;
using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class RoleView : IDisposable, INotifyPropertyChanged
    {
        //LATER audit the use of all converters, because I suspect some of them can be implemented more cleanly elsewhere
        bool disposed = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Role Role { get; init; } //LATER this should be private
        
        public CountByGroup[] CountByGroups { get; init; }

        public uint TotalCount => CountByGroups.Select(cbg => cbg.Count).Sum();

        public ICollection<Requirement> Requirements => Role.Requirements;

        public string CommaSeparatedRequirements => string.Join(", ", Role.Requirements.Select(r => r.Name));

        public RoleView(Role role, CastGroup[] cast_groups)
        {
            Role = role;
            CountByGroups = new CountByGroup[cast_groups.Length];
            for (var i = 0; i < cast_groups.Length; i++)
            {
                if (role.CountByGroups.SingleOrDefault(cbg => cbg.CastGroup == cast_groups[i]) is not CountByGroup cbg)
                {
                    cbg = new CountByGroup { CastGroup = cast_groups[i], Count = 0 };
                    role.CountByGroups.Add(cbg);
                }
                CountByGroups[i] = cbg;
                cbg.PropertyChanged += CountByGroup_PropertyChanged;
            }
            if (role.Requirements is ObservableCollection<Requirement> requirements)
                requirements.CollectionChanged += Requirements_CollectionChanged;
        }

        private void Requirements_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => OnPropertyChanged(nameof(CommaSeparatedRequirements));

        private void CountByGroup_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CountByGroups));
            OnPropertyChanged(nameof(TotalCount));
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void Dispose()
        {
            if (!disposed)
            {
                foreach (var cbg in CountByGroups)
                    cbg.PropertyChanged -= CountByGroup_PropertyChanged;
                if (Role.Requirements is ObservableCollection<Requirement> requirements)
                    requirements.CollectionChanged -= Requirements_CollectionChanged;
                disposed = true;
            }
        }
    }
}
