using Carmen.CastingEngine.Base;
using Carmen.ShowModel.Applicants;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Dummy
{
    /// <summary>
    /// For testing only, does not nessesarily produce valid casting, only valid at a data model/type level.
    /// </summary>
    public class DummyApplicantEngine : ApplicantEngine
    {
        /// <summary>Dummy value is always 100</summary>
        public override int MaxOverallAbility => 100;

        /// <summary>Dummy value is always 0</summary>
        public override int MinOverallAbility => 0;

        /// <summary>Dummy value is the weighted average of abilities, not checking for any missing values</summary>
        public override int OverallAbility(Applicant applicant)
            => Convert.ToInt32(applicant.Abilities.Sum(a => (double)a.Mark / a.Criteria.MaxMark * a.Criteria.Weight));
    }
}
