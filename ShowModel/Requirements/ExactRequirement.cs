using System;

namespace Carmen.ShowModel.Requirements
{
    public abstract class ExactRequirement : Requirement
    {
        private uint requiredValue;
        public uint RequiredValue
        {
            get => requiredValue;
            set
            {
                if (requiredValue == value)
                    return;
                requiredValue = value;
                OnPropertyChanged();
            }
        }
    }
}
