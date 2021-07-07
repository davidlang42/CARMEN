using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Model
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
        /// <summary>Indicates that a member of this group cannot be in any other groups marked as mutually exclusive</summary>
        public bool MutuallyExclusive { get; set; }
        public virtual Image? Icon { get; set; }
        public virtual ICollection<Applicant> Members { get; private set; } = new ObservableCollection<Applicant>();
        /// <summary>The number of applicants which should be allocated to this group</summary>
        public uint? RequiredCount { get; set; }
        //TODO public virtual ICollection<Requirement<Applicant>> Requirements {get;set;} = new ObservableCollection<Requirement<Applicant>>();
    }
}
