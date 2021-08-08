using ShowModel.Applicants;
using ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastingEngine
{
    /// <summary>
    /// Interface for CastingEngine functions relating to Cast selection
    /// </summary>
    public interface ISelectionEngine
    {
        /// <summary>Select applicants into cast groups, respecting those already placed
        /// NOTE: CastGroup requirements may not depend on CastGroups or Tags</summary>
        void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups);//TODO CALL

        /// <summary>Balance applicants between alternative casts, respecting those already set
        /// NOTE: All applicants must have a CastGroup set</summary>
        void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, IEnumerable<AlternativeCast> alternative_casts, IEnumerable<SameCastSet> same_cast_sets);//TODO CALL

        /// <summary>Allocate cast numbers, respecting those already set
        /// NOTE: All applicants must have a CastGroup (and AlternativeCast when CastGroup.AlternateCasts) set</summary>
        void AllocateCastNumbers(IEnumerable<Applicant> applicants, Criteria order_by, ListSortDirection sort_direction = ListSortDirection.Ascending);//TODO CALL

        /// <summary>Apply tags to applicants, respecting those already applied
        /// NOTE: Tag requirements may depend on other Tags, as long as there is no circular
        /// dependency and that tag is also being applied as part of this call</summary>
        void ApplyTags(IEnumerable<Applicant> applicants, IEnumerable<Tag> tags);//TODO CALL
    }
}
