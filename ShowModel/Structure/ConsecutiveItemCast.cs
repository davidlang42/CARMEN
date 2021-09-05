using Carmen.ShowModel.Applicants;
using System.Collections.Generic;

namespace Carmen.ShowModel.Structure
{
    public struct ConsecutiveItemCast
    {
        public Item Item1;
        public Item Item2;
        public HashSet<Applicant> Cast;
    }
}
