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
    /// An alternating cast within a cast group for which casting should be duplicated.
    /// </summary>
    public class AlternativeCast
    {
        [Key]
        public int AlternativeCastId { get; private set; }
        public string Name { get; set; } = "";
        public char Initial { get; set; } = 'A';
        public virtual ICollection<CastGroup> CastGroups { get; private set; } = new ObservableCollection<CastGroup>();
        public virtual ICollection<Applicant> Members { get; private set; } = new ObservableCollection<Applicant>();
    }
}
