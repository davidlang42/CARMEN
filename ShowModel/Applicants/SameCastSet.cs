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
            AlternativeCast? common = null;
            foreach (var applicant in Applicants)
            {
                if (applicant.CastGroup is not CastGroup cg)
                    continue; // skip not accepted applicants
                if (!cg.AlternateCasts)
                    continue; // skip if CastGroup doesn't alternate casts
                if (applicant.AlternativeCast is not AlternativeCast ac)
                    continue; // skip if AlternativeCast not set yet
                if (common == null)
                    common = ac;
                else if (common != ac)
                    return false;
            }
            return true;
        }
    }
}
