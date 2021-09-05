using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Applicants
{
    /// <summary>
    /// A set of Applicants which must be kept in the same AlternativeCast
    /// </summary>
    public class SameCastSet
    {
        [Key]
        public int SameCastSetId { get; private set; }

        private ObservableCollection<Applicant> applicants = new();
        public virtual ICollection<Applicant> Applicants => applicants;

        public bool VerifyAlternativeCasts()
        {
            //TODO implement VerifyAlternativeCasts
            throw new NotImplementedException();
        }
    }
}
