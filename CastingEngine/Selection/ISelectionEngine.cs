using Carmen.CastingEngine.Audition;
using Carmen.ShowModel.Applicants;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Selection
{
    /// <summary>
    /// Interface for CastingEngine functions relating to Cast selection
    /// </summary>
    public interface ISelectionEngine
    {
        /// <summary>Calculate the suitability of an applicant for a cast group, regardless of whether they meet all requirements.
        /// Value returned will be between 0 and 1 (inclusive). This may contain logic specific to cast groups, and is therefore
        /// different to <see cref="IAuditionEngine.SuitabilityOf(Applicant, Requirement)"/>.</summary>
        double SuitabilityOf(Applicant applicant, CastGroup cast_group);

        /// <summary>Calculate the suitability of an applicant for a tag, regardless of whether they meet all requirements.
        /// Value returned will be between 0 and 1 (inclusive). This may contain logic specific to tags, and is therefore
        /// different to <see cref="IAuditionEngine.SuitabilityOf(Applicant, Requirement)"/>.</summary>
        double SuitabilityOf(Applicant applicant, Tag tag);

        /// <summary>Set same cast sets for family groups within the applicants provided.</summary>
        Task<List<SameCastSet>> DetectFamilies(IEnumerable<Applicant> applicants);

        /// <summary>Select applicants into cast groups, respecting those already placed
        /// NOTE: CastGroup requirements may not depend on Tags</summary>
        Task SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups);

        /// <summary>Balance applicants between alternative casts, respecting those already set
        /// NOTE: This will clear the AlternativeCast of rejected Applicants</summary>
        Task BalanceAlternativeCasts(IEnumerable<Applicant> applicants, IEnumerable<SameCastSet> same_cast_sets);

        /// <summary>Allocate cast numbers, respecting those already set, ordered by a criteria, otherwise overall ability
        /// NOTE:
        /// - This will clear the cast number of rejected Applicants
        /// - Accepted applicants must have an AlternativeCast set when CastGroup.AlternateCasts == true</summary>
        Task AllocateCastNumbers(IEnumerable<Applicant> applicants);

        /// <summary>Apply tags to applicants, respecting those already applied
        /// NOTE:
        /// - This will remove Tags from rejected Applicants
        /// - Accepted applicants must have an AlternativeCast set when CastGroup.AlternateCasts == true
        /// - Tag requirements may depend on other Tags as long as there is no circular dependency and the dependee
        ///   tag is also being applied as part of this call</summary>
        Task ApplyTags(IEnumerable<Applicant> applicants, IEnumerable<Tag> tags);

        #region Passthrough of IAuditionEngine functions
        /// <summary>Calculate the overall ability of an applicant as a suitability
        /// value between 0 and 1 (inclusive).</summary>
        double OverallSuitability(Applicant applicant);

        /// <summary>A callback for when the user selects cast into cast groups manually, providing
        /// information to the engine which can be used to improve future recommendations.</summary>
        Task UserSelectedCast(IEnumerable<Applicant> applicants_accepted, IEnumerable<Applicant> applicants_rejected);
        #endregion
    }
}
