using System;

namespace ShowModel.Requirements
{
    public abstract class ExactRequirement : Requirement //LATER implement INotifyPropertyChanged for completeness
    {
        public uint RequiredValue { get; set; }
    }
}
