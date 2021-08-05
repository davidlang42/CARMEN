using ShowModel.Applicants;
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
    public class ItemView : IDisposable, INotifyPropertyChanged
    {
        bool disposed = false;

        int castGroupCount;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Item Item { get; init; }

        public ObservableCollection<RoleView> Roles { get; init; }

        public uint[] SumOfRolesCount
        {
            get
            {
                var sum = new uint[castGroupCount];
                foreach (var role in Roles)
                    for (var i = 0; i < castGroupCount; i++)
                        sum[i] += role.CountByGroups[i].Count;
                return sum;
            }
        }

        public uint SumOfRolesTotal => SumOfRolesCount.Sum();

        //TODO confirm that changing role count updates role total, role sum counts and role sum total
        //TODO confirm that changing item count updates item total, and that its only set if all 4 are set (including to 0)
        public NullableCountByGroup[] CountByGroups { get; init; }//TODO on change, trigger change of TotalCount

        /// <summary>The sum of CountByGroups.Count, if they are all set, otherwise null</summary>
        public uint? TotalCount
        {
            get
            {
                uint sum = 0;
                foreach (var cbg in CountByGroups)
                    if (cbg.Count == null)
                        return null;
                    else
                        sum += cbg.Count.Value;
                return sum;
            }
        }

        public ItemView(Item item, CastGroup[] cast_groups)
        {
            Item = item;
            Roles = new ObservableCollection<RoleView>(item.Roles.Select(r => new RoleView(r, cast_groups)));
            Roles.CollectionChanged += RoleViews_CollectionChanged;
            CountByGroups = cast_groups.Select(cg => new NullableCountByGroup(item.CountByGroups, cg)).ToArray();
            if (Item.Roles is ObservableCollection<Role> roles)
                roles.CollectionChanged += ItemRoles_CollectionChanged;
            castGroupCount = cast_groups.Length;
        }

        private void RoleViews_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            //TODO add/remove property changed handlers to each RoleView (also on initialise)
            //TODO on change of RoleView.CountByGroups trigger change of SumOfRolesCount, SumOfRolesTotal
        }

        private void ItemRoles_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            //TODO propogate changes to RoleView collection
        }
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void Dispose()
        {
            if (!disposed)
            {
                if (Item.Roles is ObservableCollection<Role> roles)
                    roles.CollectionChanged -= ItemRoles_CollectionChanged;
                disposed = true;
            }
        }
    }
}
