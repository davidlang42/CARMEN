using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Model.Applicants;
using Model.Requirements;

namespace Model.Structure
{
    public class Role : ICounted
    {
        #region Database fields
        [Key]
        public int RoleId { get; private set; }
        public string Name { get; set; } = "";
        public virtual ICollection<Item> Items { get; private set; } = new ObservableCollection<Item>();
        public virtual ICollection<Requirement> Requirements { get; set; } = new ObservableCollection<Requirement>();
        public virtual ICollection<CountByGroup> CountByGroups { get; private set; } = new ObservableCollection<CountByGroup>();
        /// <summary>This indicates that this Role's CountByGroups can be varied by the casting engine
        /// in order to meet SectionType flags or Item/Section/ShowRoot total CountByGroup requirements.
        /// Counts will only be changed for CastGroups which have existing non-zero counts, and will never
        /// be programmatically reduced to zero.</summary>
        public bool VariableCounts { get; set; }
        #endregion

        public uint CountFor(CastGroup group)
            => CountByGroups.Where(c => c.CastGroup == group).Select(c => c.Count).SingleOrDefault(); // defaults to 0
    }
}
