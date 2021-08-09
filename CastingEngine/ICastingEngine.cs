using System;
using System.Collections.Generic;
using System.Linq;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;

namespace Carmen.CastingEngine
{
    /// <summary>
    /// Interface for all CastingEngine functions
    /// </summary>
    public interface ICastingEngine : IApplicantEngine, ISelectionEngine, IAllocationEngine
    { }
}
