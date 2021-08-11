using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class ChildView : IDisposable, INotifyPropertyChanged
    {
        //LATER this class has a lot of code copied from RoleView/RoleOnlyView, there should probably be some inheritance (similarly ShowRootOrSectionView from ItemView)
        bool disposed = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Node Node { get; init; }//LATER this should probably be private

        public CastGroup[] CastGroups { get; init; }

        /// <summary>Indicies match CastGroups</summary>
        public NullableCountByGroup[] CountByGroups { get; init; }

        public uint TotalCount => CountByGroups.Select(cbg => cbg.Count ?? 0).Sum();

        public ChildView(Node node, CastGroup[] cast_groups)
        {
            Node = node;
            CastGroups = cast_groups;
            CountByGroups = new NullableCountByGroup[cast_groups.Length];
            for (var i = 0; i < cast_groups.Length; i++)
            {
                CountByGroups[i] = new NullableCountByGroup(node.CountByGroups, cast_groups[i]);
                CountByGroups[i].PropertyChanged += CountByGroup_PropertyChanged;
            }
        }

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
                disposed = true;
            }
        }
    }
}
