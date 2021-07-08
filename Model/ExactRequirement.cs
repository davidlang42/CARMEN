using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public abstract class ExactRequirement : Requirement
    {
        public uint RequiredValue { get; set; }
    }

    public class GenderRequirement : ExactRequirement
    {
        public override bool IsSatisfiedBy(Applicant applicant)
            => (uint)applicant.Gender == RequiredValue;
    }

    public class AbilityExactRequirement : ExactRequirement //TODO split classes into separate file?
    {
        public virtual Criteria Criteria { get; set; } = null!;

        public override bool IsSatisfiedBy(Applicant applicant)
            => applicant.GetMarkFor(Criteria) == RequiredValue;
    }
}
