using ShowModel.Applicants;

namespace ShowModel.Requirements
{
    public class AgeRequirement : RangeRequirement
    {
        public override bool IsSatisfiedBy(Applicant applicant)
            => IsInRange(applicant.AgeToday); //LATER use age at show date, if possible
    }
}
