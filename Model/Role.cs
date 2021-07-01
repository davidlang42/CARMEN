using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public virtual List<Item> Items { get; set; }
        //TODO Count (by Group)?
        //TODO public virtual List<Requirement> Requirements {get;set;}
    }
}
