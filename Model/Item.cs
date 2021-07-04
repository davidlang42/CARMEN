using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;

namespace Model
{
    /// <summary>
    /// An item within the show.
    /// </summary>
    public class Item : Node
    {
        public virtual ICollection<Role> Roles { get; private set; } = new ObservableCollection<Role>();

        internal override IEnumerable<Item> ItemsInOrder() => new[] { this };
    }
}
