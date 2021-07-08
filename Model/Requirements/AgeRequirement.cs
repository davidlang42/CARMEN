using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Requirements
{
    public class AgeRequirement : RangeRequirement
    {
        public override bool IsSatisfiedBy(Applicant applicant)
            => IsInRange(applicant.AgeToday()); //TODO should be age at show
    }
}
