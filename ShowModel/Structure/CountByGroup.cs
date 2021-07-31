using Microsoft.EntityFrameworkCore;
using ShowModel.Applicants;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace ShowModel.Structure
{
    /// <summary>
    /// The count of applicants required for a role from a certain group.
    /// </summary>
    [Owned]
    public class CountByGroup //LATER implement INotifyPropertyChanged for completeness
    {
        public virtual CastGroup CastGroup { get; set; } = null!;
        /// <summary>The number of applicants required of this CastGroup</summary>
        public uint Count { get; set; }
    }
}
