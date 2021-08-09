using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine
{
    /// <summary>
    /// Interface for CastingEngine functions relating to Applicant abilities
    /// </summary>
    public interface IApplicantEngine
    {
        /// <summary>Calculate the overall ability of an applicant</summary>
        int OverallAbility(Applicant applicant);
    }
}
