using Carmen.ShowModel.Applicants;

namespace Carmen.ShowModel.Requirements
{
    public class TagRequirement : Requirement
    {
        internal int RequiredTagId { get; set; } // DbSet.Load() throws IndexOutOfRangeException if foreign key is not defined

        private Tag requiredTag = null!;
        public virtual Tag RequiredTag
        {
            get => requiredTag;
            set
            {
                if (requiredTag == value)
                    return;
                requiredTag = value;
                OnPropertyChanged();
            }
        }

        public override bool IsSatisfiedBy(Applicant applicant)
            => applicant.Tags.Contains(RequiredTag);
    }
}
