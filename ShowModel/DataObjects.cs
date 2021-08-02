using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowModel
{
    /// <summary>
    /// An flagged enum representing the various objects of the model.
    /// This should contain a value for each DbSet exposed by the ShowContext.
    /// </summary>
    [Flags]
    public enum DataObjects
    {
        None = 0,
        Applicants = 1,
        AlternativeCasts = 2,
        CastGroups = 4,
        Tags = 8,
        Criterias = 16,
        Requirements = 32,
        Nodes = 64,
        SectionTypes = 128,
        Images = 256,
        All = 511
    }
}
