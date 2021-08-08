using ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastingEngine
{
    /// <summary>
    /// Interface for CastingEngine functions relating to Applicant abilities
    /// </summary>
    public interface IApplicantEngine
    {
        /// <summary>Calculate the overall ability of an applicant</summary>
        int OverallAbility(Applicant applicant); //TODO remove OverallAbility from Applicant and CALL this
    }
}
