using ShowModel.Applicants;

namespace ShowModel.Requirements
{
    public class AgeRequirement : RangeRequirement
    {
        public override bool IsSatisfiedBy(Applicant applicant)
            => IsInRange(applicant.AgeToday()); //TODO implement age at show
    }
}
