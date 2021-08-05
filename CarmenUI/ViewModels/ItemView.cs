using ShowModel.Applicants;
using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class ItemView : IDisposable//TODO INotifyPropertyChanged
    {
        private Item? item;
        public Item Item => item ?? throw new ApplicationException("Tried to use ItemView after disposed.");

        public ObservableCollection<RoleView> Roles { get; init; }

        //TODO totals?

        public ItemView(Item item, CastGroup[] cast_groups)
        {
            this.item = item;
            Roles = new ObservableCollection<RoleView>(item.Roles.Select(r => new RoleView(r, cast_groups)));
            if (item.Roles is ObservableCollection<Role> roles)
                roles.CollectionChanged += ItemRoles_CollectionChanged;
        }

        private void ItemRoles_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            //TODO propogate changes to RoleView collection
        }

        public void Dispose()
        {
            if (item != null)
            {
                if (item.Roles is ObservableCollection<Role> roles)
                    roles.CollectionChanged -= ItemRoles_CollectionChanged;
                item = null;
            }
        }
    }
}
