using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Text;

namespace Model
{
    /// <summary>
    /// The count of applicants required for a role from a certain group.
    /// </summary>
    [Owned]
    public class CountByGroup
    {
        internal static Expression<Func<CountByGroup, uint?>> CountExpression = c => c.count;

        const uint DEFAULT_COUNT = 0;

        #region Database fields
        public virtual CastGroup CastGroup { get; set; } = null!;
        private uint? count = DEFAULT_COUNT; // null means include everyone
        #endregion

        /// <summary>The number of applicants required of this CastGroup</summary>
        [NotMapped]
        public uint Count
        {
            get => count ?? (uint)CastGroup.Members.Count;
            set => count = value;
        }

        /// <summary>When true, Count is overriden to be the current number of members of this CastGroup</summary>
        [NotMapped]
        public bool Everyone
        {
            get => count == null;
            set
            {
                if (value)
                    count = null;
                else if (count == null)
                    count = DEFAULT_COUNT;
                // else: do nothing
            }
        }
    }
}
