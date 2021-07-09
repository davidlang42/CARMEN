using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Criterias;

namespace Model.Requirements
{
    public class AbilityRangeRequirement : RangeRequirement
    {
        public virtual Criteria Criteria { get; set; } = null!;
        public bool ScaleSuitability { get; set; }

        public override bool IsSatisfiedBy(Applicant applicant)
            => IsInRange(applicant.MarkFor(Criteria));

        public override double SuitabilityOf(Applicant applicant)
        {
            var mark = applicant.MarkFor(Criteria);
            if (!IsInRange(mark))
                return 0;
            else if (ScaleSuitability)
                return mark / Criteria.MaxMark;//TODO validate applicant ability not > MaxMark and MaxMark != 0
            else
                return 1;
        }
    }
}
