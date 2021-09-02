using System;
using System.ComponentModel;

namespace Carmen.ShowModel.Requirements
{
    public abstract class RangeRequirement : Requirement
    {
        private uint? minimum;
        public uint? Minimum
        {
            get => minimum;
            set
            {
                if (minimum == value)
                    return;
                minimum = value;
                OnPropertyChanged();
            }
        }

        private uint? maximum;
        public uint? Maximum
        {
            get => maximum;
            set
            {
                if (maximum == value)
                    return;
                maximum = value;
                OnPropertyChanged();
            }
        }

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
