using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Requirements
{
    public class GenderRequirement : ExactRequirement
    {
        public override bool IsSatisfiedBy(Applicant applicant)
            => (uint)applicant.Gender == RequiredValue;
    }
}
