using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;

namespace Carmen.ShowModel.Requirements
{
    public class AbilityRangeRequirement : RangeRequirement, ICriteriaRequirement //LATER implement INotifyPropertyChanged for completeness
    {
        internal int CriteriaId { get; private set; } // DbSet.Load() throws IndexOutOfRangeException if foreign key is not defined
        public virtual Criteria Criteria { get; set; } = null!;
        public bool ScaleSuitability { get; set; }

        public override bool IsSatisfiedBy(Applicant applicant)
            => IsInRange(applicant.MarkFor(Criteria));
    }
}
