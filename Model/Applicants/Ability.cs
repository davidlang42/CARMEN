using System;
using Model.Criterias;

namespace Model.Applicants
{
    /// <summary>
    /// The assessed ability of an applicant in a certain criteria.
    /// </summary>
    public class Ability
    {
        public virtual Applicant Applicant { get; set; } = null!;
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
