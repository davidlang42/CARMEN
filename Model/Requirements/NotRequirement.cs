using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Requirements
{
    public class NotRequirement : Requirement
    {
        public virtual Requirement SubRequirement { get; set; } = null!;

        public NotRequirement(Requirement requirement)
        {
            SubRequirement = requirement;
        }

        public override bool IsSatisfiedBy(Applicant applicant)
            => !SubRequirement.IsSatisfiedBy(applicant);

        public override double SuitabilityOf(Applicant applicant)
            => 1 - SubRequirement.SuitabilityOf(applicant);
    }
}
