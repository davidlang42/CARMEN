using System;
using System.Collections.Generic;
using System.Linq;
using ShowModel;
using ShowModel.Applicants;
using ShowModel.Criterias;
using ShowModel.Structure;

namespace CastingEngine
{
    /// <summary>
    /// Interface for all CastingEngine functions
    /// </summary>
    public interface ICastingEngine : IApplicantEngine, ISelectionEngine, IAllocationEngine
    { }
}
