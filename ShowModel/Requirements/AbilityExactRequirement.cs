using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;

namespace Carmen.ShowModel.Requirements
{
    public class AbilityExactRequirement : ExactRequirement, ICriteriaRequirement
    {
        internal int CriteriaId { get; private set; } // DbSet.Load() throws IndexOutOfRangeException if foreign key is not defined

        private Criteria criteria = null!;
        public virtual Criteria Criteria
        {
            get => criteria;
            set
            {
                if (criteria == value)
                    return;
                criteria = value;
                OnPropertyChanged();
            }
        }

        public override bool IsSatisfiedBy(Applicant applicant)
            => applicant.MarkFor(Criteria) == RequiredValue;
    }
}
