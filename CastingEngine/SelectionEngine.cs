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

        /// <summary>If null, order by OverallAbility</summary>
        private Criteria? castNumberOrderBy { get; set; }
        private ListSortDirection castNumberOrderDirection { get; set; }

        public abstract void AllocateCastNumbers(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, Criteria? order_by, ListSortDirection sort_direction);
        public abstract void ApplyTags(IEnumerable<Applicant> applicants, IEnumerable<Tag> tags, uint number_of_alternative_casts);
        public abstract void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, AlternativeCast[] alternative_casts, IEnumerable<SameCastSet> same_cast_sets);
        public abstract void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups, uint number_of_alternative_casts);

        public SelectionEngine(IApplicantEngine applicant_engine, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction)
        {
            ApplicantEngine = applicant_engine;
            castNumberOrderBy = cast_number_order_by;
            castNumberOrderDirection = cast_number_order_direction;
        }

        /// <summary>Default implementation returns an average of the suitability for each individual requirement</summary>
        public virtual double SuitabilityOf(Applicant applicant, CastGroup cast_group)
        {
            var sub_suitabilities = cast_group.Requirements.Select(req => ApplicantEngine.SuitabilityOf(applicant, req)).DefaultIfEmpty();
            return sub_suitabilities.Average(); //TODO handle ties with OverallAbility elsewhere
        }

        /// <summary>Default implementation returns an average of the suitability for each individual requirement</summary>
        public virtual double SuitabilityOf(Applicant applicant, Tag tag)
        {
            var sub_suitabilities = tag.Requirements.Select(req => ApplicantEngine.SuitabilityOf(applicant, req)).DefaultIfEmpty();
            return sub_suitabilities.Average(); //TODO handle ties with OverallAbility elsewhere
        }

        /// <summary>Orders the applicants based on the requested cast numbering order</summary>
        protected IEnumerable<Applicant> CastNumberingOrder(IEnumerable<Applicant> applicants)
            => (castNumberOrderBy, castNumberOrderDirection) switch
            {
                (Criteria c, ListSortDirection.Ascending) => applicants.OrderBy(a => a.MarkFor(c)),
                (Criteria c, ListSortDirection.Descending) => applicants.OrderByDescending(a => a.MarkFor(c)),
                (null, ListSortDirection.Ascending) => applicants.OrderBy(a => ApplicantEngine.OverallAbility(a)),
                (null, ListSortDirection.Descending) => applicants.OrderByDescending(a => ApplicantEngine.OverallAbility(a)),
                _ => throw new ApplicationException($"Sort type not handled: {castNumberOrderBy} / {castNumberOrderDirection}")
            };
    }
}
