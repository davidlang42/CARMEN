using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public abstract class RangeRequirement : Requirement
    {
        public uint? Minimum { get; set; }//TODO validate that at least 1 of Minimum/Maximum is not null
        public uint? Maximum { get; set; }

        protected bool IsInRange(uint value)
        {
            if (Minimum.HasValue && value < Minimum.Value)
                return false;
            if (Maximum.HasValue && value > Maximum.Value)
                return false;
            return true;
        }
    }

    public class AgeRequirement : RangeRequirement
    {
        public override bool IsSatisfiedBy(Applicant applicant)
            => IsInRange(applicant.AgeToday()); //TODO should be age at show
    }

    public class AbilityRangeRequirement : RangeRequirement //TODO split classes into separate file
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
