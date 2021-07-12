using Model.Applicants;
using System.Linq;

namespace Model.Structure
{
    /// <summary>
    /// A section of the show, containing items or other sections.
    /// </summary>
    public class Section : InnerNode
    {
        #region Database fields
        public override string Name { get; set; } = "Section";
        public virtual SectionType SectionType { get; set; } = null!;
        #endregion

        public override uint CountFor(CastGroup group)
            => CountByGroups.Where(c => c.CastGroup == group).SingleOrDefault()?.Count
            ?? ItemsInOrder().SelectMany(i => i.Roles).Distinct().Select(r => r.CountFor(group)).Sum();
    }
}
