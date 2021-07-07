using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Model
{
    public class SectionType
    {
        #region Database fields
        [Key]
        public int SectionTypeId { get; private set; }
        public string Name { get; set; } = "";
        public Image? Icon { get; set; }
        //TODO public virtual ICollection<Requirement<Section>> Requirements {get;set;} = new ObservableCollection<Requirement<Section>>();
        #endregion

        public static SectionType CreateDefault() => new() { Name = "Section" };
    }
}
