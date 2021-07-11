using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Requirements
{
    public class CastGroupRequirement : Requirement //TODO detect circular dependencies when using CastGroupRequirements on CastGroups
    {
        public virtual CastGroup RequiredGroup { get; set; } = null!;

        public override bool IsSatisfiedBy(Applicant applicant)
            => applicant.CastGroups.Contains(RequiredGroup);
    }
}
