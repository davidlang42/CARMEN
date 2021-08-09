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
        void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups, uint number_of_alternative_casts);

        /// <summary>Balance applicants between alternative casts, respecting those already set
        /// NOTE: All applicants must have a CastGroup set</summary>
        void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, IEnumerable<SameCastSet> same_cast_sets);

        /// <summary>Allocate cast numbers, respecting those already set
        /// NOTE: All applicants must have a CastGroup (and AlternativeCast when CastGroup.AlternateCasts) set</summary>
        void AllocateCastNumbers(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, Criteria? order_by, ListSortDirection sort_direction);
        //LATER might be good to be able to order by age, name, external data

        /// <summary>Apply tags to applicants, respecting those already applied
        /// NOTE: All applicants must have a CastGroup (and AlternativeCast when CastGroup.AlternateCasts) set,
        /// Tag requirements may depend on other Tags as long as there is no circular dependency and the dependee
        /// tag is also being applied as part of this call</summary>
        void ApplyTags(IEnumerable<Applicant> applicants, IEnumerable<Tag> tags);
    }
}
