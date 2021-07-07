using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// An item within the show.
    /// </summary>
    public class Item : Node
    {
        public override string Name { get; set; } = "Item";
        public virtual ICollection<Role> Roles { get; private set; } = new ObservableCollection<Role>();

        public override IEnumerable<Item> ItemsInOrder() => this.Yield();

        public Item? NextItem() => throw new NotImplementedException(); //TODO implement NextItem
        public Item? PreviousItem() => throw new NotImplementedException(); //TODO implement PreviousItem
    }
}
