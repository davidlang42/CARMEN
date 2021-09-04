using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Heuristic
{
    public class WeightedSumEngine : IApplicantEngine
    {
        public int MaxOverallAbility { get; init; }

        public int MinOverallAbility { get; init; }

        /// Calculate the overall ability of an Applicant as a simple weighted sum of their Abilities</summary>
        public int OverallAbility(Applicant applicant)
            => Convert.ToInt32(applicant.Abilities.Sum(a => (double)a.Mark / a.Criteria.MaxMark * a.Criteria.Weight));
        
        public WeightedSumEngine(Criteria[] criterias)
        {
            var max = criterias.Select(c => c.Weight).Where(w => w > 0).Sum();
            if (max > int.MaxValue)
                throw new ApplicationException($"Sum of positive Criteria weights cannot exceed {int.MaxValue}: {max}");
            MaxOverallAbility = Convert.ToInt32(max);
            var min = criterias.Select(c => c.Weight).Where(w => w < 0).Sum();
            if (min < int.MinValue)
                throw new ApplicationException($"Sum of negative Criteria weights cannot go below {int.MinValue}: {min}");
            MinOverallAbility = Convert.ToInt32(min);
            if (MinOverallAbility == MaxOverallAbility) // == 0
                MaxOverallAbility = 1; // to avoid division by zero errors
        }
    }
}
