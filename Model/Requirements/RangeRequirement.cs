using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Requirements
{
    public abstract class RangeRequirement : Requirement
    {
        public uint? Minimum { get; set; }
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
}
