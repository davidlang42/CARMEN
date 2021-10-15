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
    }
}
