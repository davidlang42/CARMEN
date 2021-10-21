using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel
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
        SameCastSets = 8,
        Tags = 16,
        Criterias = 32,
        Requirements = 64,
        Nodes = 128,
        SectionTypes = 256,
        Images = 512,
        Roles = 1024,
        Abilities = 2048,
        All = 4095
    }
}
