using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Applicants
{
    /// <summary>
    /// An applicant's identity information, for a certain identifier.
    /// </summary>
    [Owned]
    public class Identity
    {
        public virtual Identifier Identifier { get; set; } = null!;
        public string? Prefix { get; set; }
        public int? Number { get; set; }
        public string? Suffix { get; set; }

        public string DisplayText() => $"{Prefix ?? Identifier.DefaultPrefix}{Number}{Suffix ?? Identifier.DefaultSuffix}";
    }
}
