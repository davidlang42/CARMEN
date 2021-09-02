using Carmen.ShowModel.Applicants;
using System.Collections.Generic;

namespace Carmen.ShowModel.Requirements
{
    public class NotRequirement : Requirement
    {
        internal int SubRequirementId { get; private set; } // DbSet.Load() throws IndexOutOfRangeException if foreign key is not defined

        private Requirement subRequirement = null!;
        public virtual Requirement SubRequirement
        {
            get => subRequirement;
            set
            {
                if (subRequirement == value)
                    return;
                subRequirement = value;
                OnPropertyChanged();
            }
        }

        public override bool IsSatisfiedBy(Applicant applicant)
            => !SubRequirement.IsSatisfiedBy(applicant);

        internal override bool HasCircularReference(HashSet<Requirement> visited)
        {
            if (!visited.Add(this))
                return true;
            if (SubRequirement.HasCircularReference(visited))
                return true;
            visited.Remove(this);
            return false;
        }
    }
}
