using Model.Applicants;
using System.Linq;

namespace Model.Structure
{
    /// <summary>
    /// A section of the show, containing items or other sections.
    /// </summary>
    public class Section : InnerNode
    {
        public override string Name { get; set; } = "Section";
        public virtual SectionType SectionType { get; set; } = null!;
    }
}
