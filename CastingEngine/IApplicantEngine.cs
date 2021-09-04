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
        /// <summary>The maximum value an applicant's overall ability can be</summary>
        int MaxOverallAbility { get; }

        /// <summary>The minimum value an applicant's overall ability can be</summary>
        int MinOverallAbility { get; }

        /// <summary>Calculate the overall ability of an applicant</summary>
        int OverallAbility(Applicant applicant);
    }
}
