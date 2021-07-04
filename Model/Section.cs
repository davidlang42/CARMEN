using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Model
{
    public class Section : IOrdered
    {
        [Key]
        public int SectionId { get; private set; }
        public virtual Show Show { get; set; } = null!;
        public string Name { get; set; } = "";
        public virtual ICollection<Item> Items { get; private set; } = new ObservableCollection<Item>();
        public int Order { get; set; }
    }
}
