﻿using Model.Applicants;
using Model.Criterias;

namespace Model.Requirements
{
    public class AbilityExactRequirement : ExactRequirement
    {
        internal int CriteriaId { get; private set; } // DbSet.Load() throws IndexOutOfRangeException if foreign key is not defined
        public virtual Criteria Criteria { get; set; } = null!;

        public override bool IsSatisfiedBy(Applicant applicant)
            => applicant.MarkFor(Criteria) == RequiredValue;
    }
}
