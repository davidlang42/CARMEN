using Carmen.ShowModel.Applicants;
using System.Collections.Generic;

namespace Carmen.ShowModel.Structure
{
    public interface ICounted
    {
        ICollection<CountByGroup> CountByGroups { get; }

        uint CountFor(CastGroup group);
    }
}
