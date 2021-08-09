using Carmen.ShowModel.Applicants;

namespace Carmen.ShowModel.Requirements
{
    public class AgeRequirement : RangeRequirement
    {
        public override bool IsSatisfiedBy(Applicant applicant)
            => applicant.AgeToday is uint age && IsInRange(age); //LATER use age at show date, if possible
    }
}
