using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Structure
{
    /// <summary>
    /// A section of the show, containing items or other sections.
    /// </summary>
    public class Section : InnerNode, ICounted
    {
        #region Database fields
        public override string Name { get; set; } = "Section";
        public virtual SectionType SectionType { get; set; } = null!;
        public virtual ICollection<CountByGroup> CountByGroups { get; private set; } = new ObservableCollection<CountByGroup>();
        #endregion

        public uint CountFor(CastGroup group)
            => CountByGroups.Where(c => c.CastGroupId == group.CastGroupId).SingleOrDefault()?.Count
            ?? SectionType.CountByGroups.Where(c => c.CastGroupId == group.CastGroupId).SingleOrDefault()?.Count
            ?? ItemsInOrder().SelectMany(i => i.Roles).Distinct().Select(r => r.CountFor(group)).Sum();
    }
}
