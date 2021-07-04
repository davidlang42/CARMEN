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
        internal int ApplicantId { get; private set; }
        internal int CriteriaId { get; private set; }
        public virtual Criteria Criteria { get; set; } = null!;
        public uint Mark { get; set; }
    }
}
