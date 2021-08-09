using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine
{
    /// <summary>
    /// A set of applicants which must remain in the same AlternativeCast
    /// </summary>
    public class SameCastSet : HashSet<Applicant>
    { }
}
