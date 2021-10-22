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

        internal override bool HasCircularReference(HashSet<Requirement> path, HashSet<Requirement> visited)
        {
            visited.Add(this);
            if (!path.Add(this))
                return true;
            if (SubRequirement.HasCircularReference(path, visited))
                return true;
            path.Remove(this);
            return false;
        }

        public override IEnumerable<string> Validate()
        {
            if (SubRequirement == null)
                yield return $"Requirement '{Name}' has no sub requirement.";
            foreach (var base_issue in base.Validate())
                yield return base_issue;
        }
    }
}
