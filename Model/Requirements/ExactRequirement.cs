using System;

namespace Model.Requirements
{
    public abstract class ExactRequirement : Requirement
    {
        public uint RequiredValue { get; set; }
    }
}
