using Model.Applicants;
using Model.Criterias;

namespace Model.Requirements
{
    public class AbilityRangeRequirement : RangeRequirement
    {
        internal int CriteriaId { get; private set; } // DbSet.Load() throws IndexOutOfRangeException if foreign key is not defined
        public virtual Criteria Criteria { get; set; } = null!;
        public bool ScaleSuitability { get; set; }

        public override bool IsSatisfiedBy(Applicant applicant)
            => IsInRange(applicant.MarkFor(Criteria));

        public override double SuitabilityOf(Applicant applicant)
        {
            var mark = applicant.MarkFor(Criteria);
            if (!IsInRange(mark))
                return 0;
            else if (ScaleSuitability)
                return mark / Criteria.MaxMark;
            else
                return 1;
        }
    }
}
