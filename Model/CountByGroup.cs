﻿using System;
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
        internal virtual int RoleId { get; private set; }
        internal virtual int CastGroupId { get; private set; }
        public virtual CastGroup CastGroup { get; set; } = null!;
        public uint Count { get; set; }
    }
}
