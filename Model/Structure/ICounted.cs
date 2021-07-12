using Model.Applicants;
using System.Collections.Generic;

namespace Model.Structure
{
    public interface ICounted
    {
        ICollection<CountByGroup> CountByGroups { get; }

        uint CountFor(CastGroup group);
    }
}
