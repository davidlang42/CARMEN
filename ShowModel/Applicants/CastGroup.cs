using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ShowModel.Requirements;

namespace ShowModel.Applicants
{
    /// <summary>
    /// A group of people which an applicant can be selected into.
    /// Cast groups are mutually exclusive.
    /// </summary>
    public class CastGroup : IOrdered, INamed
    {
        [Key]
        public int CastGroupId { get; private set; }
        public int Order { get; set; }
        private string name = "";
        public string Name
        {
            get => name;
            set
            {
                name = value;
                Abbreviation = value.Abbreviate();
            }
        }
        public string Abbreviation { get; set; } = "";
        public virtual Image? Icon { get; set; }
        public virtual ICollection<Applicant> Members { get; private set; } = new ObservableCollection<Applicant>();
        /// <summary>The number of applicants which should be allocated to this group</summary>
        public uint? RequiredCount { get; set; }
        public virtual ICollection<AlternativeCast> AlternativeCasts { get; private set; } = new ObservableCollection<AlternativeCast>();
        public virtual ICollection<Requirement> Requirements { get; private set; } = new ObservableCollection<Requirement>();
#endregion

        public bool AlternatingCasts() => AlternativeCasts.Any();
    }
}
