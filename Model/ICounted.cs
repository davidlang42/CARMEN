using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public interface ICounted
    {
        ICollection<CountByGroup> CountByGroups { get; }

        uint CountFor(CastGroup group);
    }
}
