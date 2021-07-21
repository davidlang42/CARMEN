using ShowModel.Applicants;
using System.Collections.Generic;

namespace ShowModel.Structure
{
    public interface ICounted
    {
        ICollection<CountByGroup> CountByGroups { get; }

        uint CountFor(CastGroup group);
    }
}
