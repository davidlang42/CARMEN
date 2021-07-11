using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Model.Criterias;

namespace Model
{
    /// <summary>
    /// The assessed ability of an applicant in a certain criteria.
    /// </summary>
    [Owned]
    public class Ability
    {
        internal virtual Applicant Applicant { get; private set; } = null!;
        public virtual Criteria Criteria { get; set; } = null!;
        private uint mark;
        public uint Mark {
            get => mark;
            set
            {
                if (value > Criteria.MaxMark)
                    throw new ArgumentException($"{nameof(Mark)} cannot be greater than Criteria.MaxMark ({Criteria.MaxMark})");
                mark = value;
            }
        }
    }
}
