using Carmen.ShowModel.Applicants;
using System.Collections.Generic;

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

        public override IEnumerable<string> Validate()
        {
            if (RequiredTag == null)
                yield return $"Requirement '{Name}' has no required tag.";
            foreach (var base_issue in base.Validate())
                yield return base_issue;
        }
    }
}
