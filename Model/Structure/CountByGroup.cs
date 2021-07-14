using Microsoft.EntityFrameworkCore;
using Model.Applicants;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace Model.Structure
{
    /// <summary>
    /// The count of applicants required for a role from a certain group.
    /// </summary>
    [Owned]
    public class CountByGroup
    {
        public virtual CastGroup CastGroup { get; set; } = null!;
        /// <summary>The number of applicants required of this CastGroup, or null if FillRemaining is true.</summary>
        public uint Count { get; set; }
    }
}
