using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Model
{
    public class Section : IOrdered
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public virtual ICollection<Item> Items { get; private set; } = new ObservableCollection<Item>();
        public int Order { get; set; }
    }
}
