using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Heuristic
{
    public class WeightedSumEngine : IApplicantEngine
    {
        /// Calculate the overall ability of an Applicant as a simple weighted sum of their Abilities</summary>
        public int OverallAbility(Applicant applicant)
        {
            try
            {
                return Convert.ToInt32(applicant.Abilities.Sum(a => (double)a.Mark / a.Criteria.MaxMark * a.Criteria.Weight));
            }
            catch (OverflowException)
            {
                //LATER log exception
                return int.MaxValue;
            }
            catch (DivideByZeroException)
            {
                //LATER log exception
                return 0;
            }
        }
    }
}
