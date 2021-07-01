using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Model
{
    public class Item : IOrdered
    {
        public Guid ItemId { get; set; }
        public virtual Section Section { get; set; } = null!;
        public string Name { get; set; } = "";
        public int Order { get; set; }
        public virtual ICollection<Role> Roles { get; private set; } = new ObservableCollection<Role>();
    }
}
