using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Requirements
{
    /// <summary>
    /// Abstraction of a requirement's overall ability weight
    /// NOTE: Not guaranteed to be of type Requirement
    /// </summary>
    public interface IOverallWeighting
    {
        public double OverallWeight { get; set; }
    }
}
