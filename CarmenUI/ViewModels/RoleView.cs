using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
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
    public abstract class RoleView : IDisposable, INotifyPropertyChanged
    {
        //LATER audit the use of all converters, because I suspect some of them can be implemented more cleanly elsewhere
        bool disposed;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Role Role { get; init; } //LATER this should be protected

        public ICollection<Requirement> Requirements => Role.Requirements;

        public string? CommaSeparatedRequirements => Role.Requirements.Count == 0 ? null : string.Join(", ", Role.Requirements.InOrder().Select(r => r.Name));

        public string? CommaSeparatedOtherRequirements
        {
            get
            {
                var non_primary_requirements = Role.Requirements.Where(r => !r.Primary).ToArray();
                if (non_primary_requirements.Length == 0)
                    return null;
                return string.Join(", ", non_primary_requirements.InOrder().Select(r => r.Name));
            }
        }

        public RoleView(Role role)
        {
            Role = role;
            if (role.Requirements is ObservableCollection<Requirement> requirements)
                requirements.CollectionChanged += Requirements_CollectionChanged;
        }

        private void Requirements_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CommaSeparatedRequirements));
            OnPropertyChanged(nameof(CommaSeparatedOtherRequirements));
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>Actually dispose change handlers, etc.
        /// This will only be called once.</summary>
        protected virtual void DisposeInternal()
        {
            if (Role.Requirements is ObservableCollection<Requirement> requirements)
                requirements.CollectionChanged -= Requirements_CollectionChanged;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                DisposeInternal();
                disposed = true;
            }
        }
    }
}
