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

        private CastGroup[] castGroups;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Item Item { get; init; } //LATER this should be private

        public ObservableCollection<RoleView> Roles { get; init; }

        public uint[] SumOfRolesCount
        {
            get
            {
                var sum = new uint[castGroups.Length];
                foreach (var role in Roles)
                    for (var i = 0; i < sum.Length; i++)
                        sum[i] += role.CountByGroups[i].Count;
                return sum;
            }
        }

        public uint SumOfRolesTotal => SumOfRolesCount.Sum();

        public NullableCountByGroup[] CountByGroups { get; init; }

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
            Roles = new ObservableCollection<RoleView>(item.Roles.Select(r =>
            {
                var rv = new RoleView(r, cast_groups);
                rv.PropertyChanged += RoleView_PropertyChanged;
                return rv;
            }));
            Roles.CollectionChanged += Roles_CollectionChanged;
            CountByGroups = cast_groups.Select(cg =>
            {
                var ncbg = new NullableCountByGroup(item.CountByGroups, cg);
                ncbg.PropertyChanged += NullableCountByGroup_PropertyChanged;
                return ncbg;
            }).ToArray();
            this.castGroups = cast_groups;
        }

        private void NullableCountByGroup_PropertyChanged(object? sender, PropertyChangedEventArgs e)
            => OnPropertyChanged(nameof(TotalCount));

        private void Roles_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                        foreach (RoleView rv in e.OldItems)
                        {
                            rv.PropertyChanged -= RoleView_PropertyChanged;
                            Item.Roles.Remove(rv.Role);
                        }
                    if (e.NewItems != null)
                        foreach (RoleView rv in e.NewItems)
                        {
                            Item.Roles.Add(rv.Role);
                            rv.PropertyChanged += RoleView_PropertyChanged;
                        }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var rv in Roles)
                        rv.PropertyChanged -= RoleView_PropertyChanged;
                    Item.Roles.Clear();
                    foreach (var rv in Roles)
                    {
                        Item.Roles.Add(rv.Role);
                        rv.PropertyChanged += RoleView_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    // do nothing
                    break;
                default:
                    throw new NotImplementedException($"Action not handled: {e.Action}");
            }
            
        }

        private void RoleView_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(RoleView.CountByGroups))
            {
                OnPropertyChanged(nameof(SumOfRolesCount));
                OnPropertyChanged(nameof(SumOfRolesTotal));
            }
        }

        public RoleView AddRole()
        {
            var role_view = new RoleView(new Role(), castGroups);
            Roles.Add(role_view);
            return role_view;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void Dispose()
        {
            if (!disposed)
            {
                Roles.CollectionChanged -= Roles_CollectionChanged;
                foreach (var rv in Roles)
                    rv.PropertyChanged -= RoleView_PropertyChanged;
                foreach (var ncbg in CountByGroups)
                    ncbg.PropertyChanged -= NullableCountByGroup_PropertyChanged;
                disposed = true;
            }
        }
    }
}
