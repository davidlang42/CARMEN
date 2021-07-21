using ShowModel.Applicants;

namespace ShowModel.Requirements
{
    public class GenderRequirement : ExactRequirement
    {
        public override bool IsSatisfiedBy(Applicant applicant)
            => applicant.Gender != null && (uint)applicant.Gender == RequiredValue;
    }
}
