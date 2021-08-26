using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
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
    public class ItemView : IDisposable, INotifyPropertyChanged
    {
        bool disposed = false;

        private CastGroup[] castGroups;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Item Item { get; init; } //LATER this should be private

        public ObservableCollection<RoleOnlyView> Roles { get; init; }

        public uint[] SumOfRolesCount
        {
            get
            {
                var sum = new uint[castGroups.Length];
                foreach (var role in Roles)
                    for (var i = 0; i < sum.Length; i++)
                        if (role.CountByGroups[i].Count is uint count)
                            sum[i] += count;
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

        public string[] CountErrorBackgroundColors
        {
            get
            {
                var colors = new string[castGroups.Length];
                for (var i = 0; i < colors.Length; i++)
                    if (CountByGroups[i].Count is uint required_count
                        && SumOfRolesCount[i] != required_count)
                        colors[i] = "LightCoral";//LATER use constants, also convert to brush
                    else
                        colors[i] = "White";//LATER use constants, also convert to brush
                return colors;
            }
        }

        public string NoRolesErrorBackgroundColor => Item.Roles.Count == 0 ? "LightCoral" : "WhiteSmoke";//LATER use constants, also convert to brush

        public ItemView(Item item, CastGroup[] cast_groups)
        {
            Item = item;
            Roles = new ObservableCollection<RoleOnlyView>(item.Roles.InOrder().Select(r =>
            {
                var rv = new RoleOnlyView(r, cast_groups, this);
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
        {
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(CountErrorBackgroundColors));
        }

        private void Roles_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                        foreach (RoleOnlyView rv in e.OldItems)
                        {
                            rv.PropertyChanged -= RoleView_PropertyChanged;
                            Item.Roles.Remove(rv.Role);
                            rv.Dispose();
                        }
                    if (e.NewItems != null)
                        foreach (RoleOnlyView rv in e.NewItems)
                        {
                            Item.Roles.Add(rv.Role);
                            rv.PropertyChanged += RoleView_PropertyChanged;
                        }
                    RoleView_PropertyChanged(this, new PropertyChangedEventArgs(""));
                    break;
                case NotifyCollectionChangedAction.Reset://LATER is this implementation correct? it probably isn't used
                    foreach (var rv in Roles)
                    {
                        rv.PropertyChanged -= RoleView_PropertyChanged;
                        rv.Dispose();
                    }
                    Item.Roles.Clear();
                    foreach (var rv in Roles)
                    {
                        Item.Roles.Add(rv.Role);
                        rv.PropertyChanged += RoleView_PropertyChanged;
                    }
                    RoleView_PropertyChanged(this, new PropertyChangedEventArgs(""));
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
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(RoleOnlyView.CountByGroups))
            {
                OnPropertyChanged(nameof(SumOfRolesCount));
                OnPropertyChanged(nameof(SumOfRolesTotal));
                OnPropertyChanged(nameof(CountErrorBackgroundColors));
                OnPropertyChanged(nameof(NoRolesErrorBackgroundColor));
            }
        }

        public RoleOnlyView AddRole(RoleOnlyView? insert_after = null)
        {
            var role_view = new RoleOnlyView(new Role(), castGroups, this);
            if (insert_after != null)
                for (var i = 0; i < Roles.Count; i++)
                    if (Roles[i] == insert_after)
                    {
                        Roles.Insert(i + 1, role_view);
                        return role_view;
                    }
            Roles.Add(role_view);
            return role_view;
        }

        public void RemoveRole(RoleOnlyView role_view)
        {
            Roles.Remove(role_view);
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
                {
                    rv.PropertyChanged -= RoleView_PropertyChanged;
                    rv.Dispose();
                }
                foreach (var ncbg in CountByGroups)
                    ncbg.PropertyChanged -= NullableCountByGroup_PropertyChanged;
                disposed = true;
            }
        }
    }
}
