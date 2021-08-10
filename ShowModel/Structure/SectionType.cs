using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Carmen.ShowModel.Structure
{
    public class SectionType : INameOrdered //LATER implement INotifyPropertyChanged for completeness
    {
        [Key]
        public int SectionTypeId { get; private set; }
        public string Name { get; set; } = "";
        public bool AllowMultipleRoles { get; set; } = false;
        public bool AllowNoRoles { get; set; } = false;
        /// <summary>AllowConsecutiveItems defaults to true in SectionType, but false in ShowRoot,
        /// so that the consecutive item check does not run multiple times by default</summary>
        public bool AllowConsecutiveItems { get; set; } = true;
        public virtual ICollection<Section> Sections { get; private set; } = new ObservableCollection<Section>(); //LATER add this to RelationshipTests.cs
    }
}
