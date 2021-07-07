using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Model
{
    public class SectionType
    {
        [Key]
        public int SectionTypeId { get; private set; }
        public string Name { get; set; } = "Section";
        public virtual Image? Icon { get; set; }
        //TODO public virtual ICollection<Requirement<Section>> Requirements {get;set;} = new ObservableCollection<Requirement<Section>>();
    }
}
