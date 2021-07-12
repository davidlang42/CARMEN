using Model.Applicants;

namespace Model.Requirements
{
    public class GenderRequirement : ExactRequirement
    {
        public override bool IsSatisfiedBy(Applicant applicant)
            => (uint)applicant.Gender == RequiredValue;
    }
}
