using ShowModel.Applicants;
using System.Linq;

namespace ShowModel.Structure
{
    /// <summary>
    /// A section of the show, containing items or other sections.
    /// </summary>
    public class Section : InnerNode //LATER implement INotifyPropertyChanged for completeness
    {
        public override string Name { get; set; } = "Section"; //LATER move back into Node
        public virtual SectionType SectionType { get; set; } = null!;
    }
}
