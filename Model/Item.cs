using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Model
{
    /// <summary>
    /// An item within the show.
    /// </summary>
    public class Item : IOrdered
    {
        public int ItemId { get; private set; }
        public virtual Section Section { get; set; } = null!;
        public string Name { get; set; } = "";
        public int Order { get; set; }
        public virtual ICollection<Role> Roles { get; private set; } = new ObservableCollection<Role>();
    }
}
