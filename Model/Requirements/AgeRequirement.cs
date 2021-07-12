using Model.Applicants;

namespace Model.Requirements
{
    public class AgeRequirement : RangeRequirement
    {
        public override bool IsSatisfiedBy(Applicant applicant)
            => IsInRange(applicant.AgeToday()); //TODO implement age at show
    }
}
