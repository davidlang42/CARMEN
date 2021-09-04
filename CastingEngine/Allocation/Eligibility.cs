using Carmen.ShowModel.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine
{
    public struct Eligibility
    {
        public Requirement[] RequirementsNotMet { get; init; }

        public bool IsEligible => RequirementsNotMet.Length == 0;
    }
}
