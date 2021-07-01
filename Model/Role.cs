using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Model
{
    public class Role
    {
        public Guid RoleId { get; set; }
        public string Name { get; set; } = "";
        public virtual ICollection<Item> Items { get; private set; } = new ObservableCollection<Item>();
        //TODO Count (by Group)?
        //TODO public virtual List<Requirement> Requirements {get;set;}
    }
}
