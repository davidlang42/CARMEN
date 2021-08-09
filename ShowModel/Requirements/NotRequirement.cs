﻿using ShowModel.Applicants;
using System.Collections.Generic;

namespace ShowModel.Requirements
{
    public class NotRequirement : Requirement //LATER implement INotifyPropertyChanged for completeness
    {
        internal int SubRequirementId { get; private set; } // DbSet.Load() throws IndexOutOfRangeException if foreign key is not defined
        public virtual Requirement SubRequirement { get; set; } = null!;

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
