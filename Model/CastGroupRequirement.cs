using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class CastGroupRequirement : Requirement
    {
        public virtual CastGroup CastGroup { get; set; } = null!;

        public override bool IsSatisfiedBy(Applicant applicant)
            => applicant.CastGroups.Contains(CastGroup);
    }
}
