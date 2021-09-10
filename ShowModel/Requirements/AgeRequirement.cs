using Carmen.ShowModel.Applicants;

namespace Carmen.ShowModel.Requirements
{
    public class AgeRequirement : RangeRequirement
    {
        public override bool IsSatisfiedBy(Applicant applicant)
            => applicant.Age is uint age && IsInRange(age);
    }
}
