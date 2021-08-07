using ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowModel.Requirements
{
    /// <summary>
    /// Indicates that requirement is based on a certain Criteria
    /// </summary>
    public interface ICriteriaRequirement
    {
        public Criteria Criteria { get; }
    }
}
