using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;

namespace Carmen.ShowModel.Requirements
{
    public class AbilityExactRequirement : ExactRequirement, ICriteriaRequirement //LATER implement INotifyPropertyChanged for completeness
    {
        internal int CriteriaId { get; private set; } // DbSet.Load() throws IndexOutOfRangeException if foreign key is not defined
        public virtual Criteria Criteria { get; set; } = null!;

        public override bool IsSatisfiedBy(Applicant applicant)
            => applicant.MarkFor(Criteria) == RequiredValue;
    }
}
