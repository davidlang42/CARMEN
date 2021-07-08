using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Requirements
{
    public class AbilityExactRequirement : ExactRequirement
    {
        public virtual Criteria Criteria { get; set; } = null!;

        public override bool IsSatisfiedBy(Applicant applicant)
            => applicant.MarkFor(Criteria) == RequiredValue;
    }
}
