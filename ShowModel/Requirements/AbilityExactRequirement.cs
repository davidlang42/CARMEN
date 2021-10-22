using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Carmen.ShowModel.Requirements
{
    public class AbilityExactRequirement : ExactRequirement, ICriteriaRequirement
    {
        internal int CriteriaId { get; private set; } // DbSet.Load() throws IndexOutOfRangeException if foreign key is not defined

        private Criteria criteria = null!;
        public virtual Criteria Criteria
        {
            get => criteria;
            set
            {
                if (criteria == value)
                    return;
                criteria = value;
                OnPropertyChanged();
            }
        }

        private double existingRoleCost = 1;
        /// <summary>The number of percentage points which each existing role subtracts
        /// from an applicant's suitability. Must be between 0 and 100 inclusive.</summary>
        public double ExistingRoleCost
        {
            get => existingRoleCost;
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 100)
                    value = 100;
                if (existingRoleCost == value)
                    return;
                existingRoleCost = value;
                OnPropertyChanged();
            }
        }

        public override bool IsSatisfiedBy(Applicant applicant)
            => applicant.MarkFor(Criteria) == RequiredValue;

        public override IEnumerable<string> Validate()
        {
            if (Criteria == null)
                yield return $"Requirement '{Name}' has no required criteria.";
            foreach (var base_issue in base.Validate())
                yield return base_issue;
        }
    }
}
