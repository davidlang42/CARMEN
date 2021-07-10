using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Model.Criterias
{
    /// <summary>
    /// A criteria on which applicants are assessed, and assigned a mark.
    /// </summary>
    public abstract class Criteria : IOrdered
    {
        [Key]
        public int CriteriaId { get; private set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int Order { get; set; }
        public double Weight { get; set; }
        private uint maxMark;
        public virtual uint MaxMark
        {
            get => maxMark;
            set
            {
                if (value == 0)
                    throw new ArgumentException($"{nameof(MaxMark)} cannot be set to 0.");
                maxMark = value; //TODO validation that new MaxMark isn't greater than any of the currently set marks
            }
        }
    }
}
