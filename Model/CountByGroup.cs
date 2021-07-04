using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    /// <summary>
    /// The count of applicants required for a role from a certain group.
    /// Groups used with counts must be mutually exclusive.
    /// Composite Key configured in <c cref="ShowContext.OnModelCreating">DbContext</c>.
    /// </summary>
    public class CountByGroup
    {
        internal virtual Role Role { get; set; } = null!;
        public virtual CastGroup Group { get; set; } = null!;
        public uint Count { get; set; }
    }
}
