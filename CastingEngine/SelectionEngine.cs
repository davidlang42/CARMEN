using Carmen.ShowModel.Applicants;
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
    /// The abstract base class of most ISelectionEngine based engines
    /// </summary>
    public abstract class SelectionEngine : ISelectionEngine
    {
        //TODO look at common arguments, eg. cast groups/ alternative casts ande decide what should be in the constructor
        public IApplicantEngine ApplicantEngine { get; init; }

        public abstract void AllocateCastNumbers(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, Criteria? order_by, ListSortDirection sort_direction);
        public abstract void ApplyTags(IEnumerable<Applicant> applicants, IEnumerable<Tag> tags, uint number_of_alternative_casts);
        public abstract void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, IEnumerable<SameCastSet> same_cast_sets);
        public abstract void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups, uint number_of_alternative_casts);

        public SelectionEngine(IApplicantEngine applicant_engine)
        {
            ApplicantEngine = applicant_engine;
        }
    }
}
