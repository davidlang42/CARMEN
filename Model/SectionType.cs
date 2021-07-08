using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;

namespace Model
{
    public class SectionType : ICounted
    {
        #region Database fields
        [Key]
        public int SectionTypeId { get; private set; }
        public string Name { get; set; } = "Section";
        public virtual Image? Icon { get; set; }
        public virtual ICollection<CountByGroup> CountByGroups { get; private set; } = new ObservableCollection<CountByGroup>();
        #endregion

        public uint CountFor(CastGroup group)
            => throw new NotImplementedException("Section types cannot be counted.");
    }
}
