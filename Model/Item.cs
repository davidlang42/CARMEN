using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class Item : IOrdered
    {
        public Guid Id { get; set; }
        public virtual Section Section { get; set; } = null!;
        public string Name { get; set; } = "";
        public int Order { get; set; }
        //TODO public virtual List<Role> Roles { get; set; }
    }
}
