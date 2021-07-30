using ShowModel.Applicants;

namespace ShowModel.Requirements
{
    public class AgeRequirement : RangeRequirement
    {
        public override bool IsSatisfiedBy(Applicant applicant)
            => applicant.AgeToday is uint age && IsInRange(age); //LATER use age at show date, if possible
    }
}
