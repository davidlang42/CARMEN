using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Linq;

namespace Carmen.CastingEngine.Audition
{
    /// <summary>
    /// A basic AuditionEngine which calculates an Applicant's overall ability as a weighted sum of their abilities.
    /// </summary>
    public class WeightedSumEngine : AuditionEngine
    {
        int maxOverallAbility;
        public override int MaxOverallAbility => maxOverallAbility;

        int minOverallAbility;
        public override int MinOverallAbility => minOverallAbility;

        public WeightedSumEngine(Criteria[] criterias)
        {
            UpdateRange(criterias);
        }

        /// <summary>Calculate the overall ability of an Applicant as a weighted sum of their Abilities.
        /// NOTE: Calculation duplicated in ApplicantReport for simplicity.</summary>
        public override int OverallAbility(Applicant applicant)
            => Convert.ToInt32(applicant.Abilities.Sum(a => (double)a.Mark / a.Criteria.MaxMark * a.Criteria.Weight));

        protected void UpdateRange(Criteria[] criterias)
        {
            var max = criterias.Select(c => c.Weight).Where(w => w > 0).Sum();
            if (max > int.MaxValue)
                throw new ApplicationException($"Sum of positive Criteria weights cannot exceed {int.MaxValue}: {max}");
            maxOverallAbility = Convert.ToInt32(max);
            var min = criterias.Select(c => c.Weight).Where(w => w < 0).Sum();
            if (min < int.MinValue)
                throw new ApplicationException($"Sum of negative Criteria weights cannot go below {int.MinValue}: {min}");
            minOverallAbility = Convert.ToInt32(min);
            if (minOverallAbility == maxOverallAbility) // == 0
                maxOverallAbility = 1; // to avoid division by zero errors
        }
    }
}
