using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Requirements;
using System.Collections.Generic;

namespace Carmen.CastingEngine.Audition
{
    /// <summary>
    /// Interface for CastingEngine functions relating to auditioning Applicants
    /// </summary>
    public interface IAuditionEngine
    {
        /// <summary>The maximum value an applicant's overall ability can be</summary>
        int MaxOverallAbility { get; }

        /// <summary>The minimum value an applicant's overall ability can be</summary>
        int MinOverallAbility { get; }

        /// <summary>Calculate the overall ability of an applicant</summary>
        int OverallAbility(Applicant applicant);

        /// <summary>A callback for when the user selects cast into cast groups manually, providing
        /// information to the engine which can be used to improve future recommendations.</summary>
        void UserSelectedCast(IEnumerable<Applicant> applicants_accepted, IEnumerable<Applicant> applicants_rejected);

        /// <summary>Calculate the suitability of an applicant against a single requirement.
        /// Value returned will be between 0 and 1 (inclusive).</summary>
        double SuitabilityOf(Applicant applicant, Requirement requirement);

        /// <summary>Calculate the overall ability of an applicant as a suitability
        /// value between 0 and 1 (inclusive).</summary>
        double OverallSuitability(Applicant applicant)
            => (OverallAbility(applicant) - MinOverallAbility) / (double)(MaxOverallAbility - MinOverallAbility);
    }
}
