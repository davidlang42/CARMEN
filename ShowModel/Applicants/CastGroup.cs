using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using ShowModel.Requirements;

namespace ShowModel.Applicants
{
    /// <summary>
    /// A group of people which an applicant can be cast into.
    /// </summary>
    public class CastGroup : IOrdered
    {
        [Key]
        public int CastGroupId { get; private set; }
        public int Order { get; set; }
        public string Name { get; set; } = "Cast";
        /// <summary>Indicates that a member of this group cannot be in any other primary groups.</summary>
        public bool Primary { get; set; }
        public virtual Image? Icon { get; set; }
        public virtual ICollection<Applicant> Members { get; private set; } = new ObservableCollection<Applicant>();
        /// <summary>The number of applicants which should be allocated to this group</summary>
        public uint? RequiredCount { get; set; }
        public virtual ICollection<Requirement> Requirements { get; set; } = new ObservableCollection<Requirement>();
    }
}
