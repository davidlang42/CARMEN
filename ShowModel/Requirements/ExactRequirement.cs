using System;

namespace ShowModel.Requirements
{
    public abstract class ExactRequirement : Requirement
    {
        public uint RequiredValue { get; set; }
    }
}
