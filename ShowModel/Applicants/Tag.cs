using ShowModel.Requirements;
using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowModel.Applicants
{
    /// <summary>
    /// A group of people which an applicant can be selected into.
    /// An applicant can have many Tags.
    /// </summary>
    public class Tag : INamed //LATER implement INotifyPropertyChanged for completeness
    {
        #region Database fields
        [Key]
        public int TagId { get; private set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public virtual Image? Icon { get; set; }
        public virtual ICollection<Applicant> Members { get; private set; } = new ObservableCollection<Applicant>();
        public virtual ICollection<Requirement> Requirements { get; private set; } = new ObservableCollection<Requirement>();
        public virtual ICollection<CountByGroup> CountByGroups { get; private set; } = new ObservableCollection<CountByGroup>();
        #endregion

        public uint? CountFor(CastGroup group)
            => CountByGroups.Where(c => c.CastGroup == group).Select(c => (uint?)c.Count).SingleOrDefault();
    }
}
