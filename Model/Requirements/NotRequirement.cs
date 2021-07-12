﻿using Model.Applicants;

namespace Model.Requirements
{
    public class NotRequirement : Requirement
    {
        internal int SubRequirementId { get; private set; } // DbSet.Load() throws IndexOutOfRangeException if foreign key is not defined
        public virtual Requirement SubRequirement { get; set; } = null!;

        public override bool IsSatisfiedBy(Applicant applicant)
            => !SubRequirement.IsSatisfiedBy(applicant);

        public override double SuitabilityOf(Applicant applicant)
            => 1 - SubRequirement.SuitabilityOf(applicant);
    }
}
