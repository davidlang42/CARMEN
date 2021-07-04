using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    /// <summary>
    /// The assessed ability of an applicant in a certain criteria.
    /// Composite Key configured in <c cref="ShowContext.OnModelCreating">DbContext</c>.
    /// </summary>
    public class Ability
    {
        public virtual Applicant Applicant { get; set; } = null!;
        public virtual Criteria Criteria { get; set; } = null!;
        public uint Mark { get; set; }
    }
}
