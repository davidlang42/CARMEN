using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Requirements
{
    /// <summary>
    /// Indicates that requirement is based on a certain Criteria
    /// </summary>
    public interface ICriteriaRequirement : IOrdered, INamed
    {
        public Criteria Criteria { get; }
        public double ExistingRoleCost { get; set; }
        public double SuitabilityWeight { get; set; }
    }
}
