using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ShowModel.Structure
{
    public class SectionType : INameOrdered //LATER implement INotifyPropertyChanged for completeness
    {
        [Key]
        public int SectionTypeId { get; private set; }
        public string Name { get; set; } = "Section";
        public virtual Image? Icon { get; set; }
        public bool AllowMultipleRoles { get; set; } = false;
        public bool AllowNoRoles { get; set; } = false;
        public virtual ICollection<Section> Sections { get; private set; } = new ObservableCollection<Section>(); //LATER add this to RelationshipTests.cs
    }
}
