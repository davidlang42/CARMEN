using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        const uint DEFAULT_COUNT = 0;

        #region Database fields
        internal virtual int RoleId { get; private set; }
        internal virtual int CastGroupId { get; private set; }
        public virtual CastGroup CastGroup { get; set; } = null!;
        protected uint? count = DEFAULT_COUNT; // null means include everyone
        #endregion

        /// <summary>The number of applicants required of this CastGroup</summary>
        [BackingField(nameof(count))]//TODO this isnt storing nulls, and Everyone is stored as an int
        public uint Count
        {
            get => count ?? (uint)CastGroup.Members.Count;
            set => count = value;
        }

        /// <summary>When true, Count is overriden to be the current number of members of this CastGroup</summary>
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
