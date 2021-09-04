﻿using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine
{
    /// <summary>
    /// Interface for CastingEngine functions relating to Cast selection
    /// </summary>
    public interface ISelectionEngine
    {
        /// <summary>An accessor to the IApplicantEngine used by this selection engine</summary>
        IApplicantEngine ApplicantEngine { get; }//LATER remove if not needed, but trust me, it will be

        /// <summary>Calculate the suitability of an applicant for a cast group, regardless of whether they meet all requirements.
        /// Value returned will be between 0 and 1 (inclusive). This may contain logic specific to cast groups, and is therefore
        /// different to IApplicantEngine.SuitabilityOf(Applicant, Requirement).</summary>
        double SuitabilityOf(Applicant applicant, CastGroup cast_group);

        /// <summary>Calculate the suitability of an applicant for a tag, regardless of whether they meet all requirements.
        /// Value returned will be between 0 and 1 (inclusive). This may contain logic specific to tags, and is therefore
        /// different to IApplicantEngine.SuitabilityOf(Applicant, Requirement).</summary>
        double SuitabilityOf(Applicant applicant, Tag tag);

        /// <summary>Select applicants into cast groups, respecting those already placed
        /// NOTE: CastGroup requirements may not depend on CastGroups or Tags</summary>
        void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups, uint number_of_alternative_casts);

        /// <summary>Balance applicants between alternative casts, respecting those already set
        /// NOTE: This will clear the AlternativeCast of rejected Applicants</summary>
        void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, IEnumerable<SameCastSet> same_cast_sets);

        /// <summary>Allocate cast numbers, respecting those already set, ordered by a criteria, otherwise overall ability
        /// NOTE:
        /// - This will clear the cast number of rejected Applicants
        /// - Accepted applicants must have an AlternativeCast set when CastGroup.AlternateCasts == true</summary>
        void AllocateCastNumbers(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, Criteria? order_by, ListSortDirection sort_direction);

        /// <summary>Apply tags to applicants, respecting those already applied
        /// NOTE:
        /// - This will remove Tags from rejected Applicants
        /// - Accepted applicants must have an AlternativeCast set when CastGroup.AlternateCasts == true
        /// - Tag requirements may depend on other Tags as long as there is no circular dependency and the dependee
        ///   tag is also being applied as part of this call</summary>
        void ApplyTags(IEnumerable<Applicant> applicants, IEnumerable<Tag> tags, uint number_of_alternative_casts);//TODO handle tag dependent tags
    }
}
