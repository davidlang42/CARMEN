using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Model
{
    /// <summary>
    /// A criteria on which applicants are assessed, and assigned a mark.
    /// </summary>
    public class Criteria : IOrdered
    {
        [Key]
        public int CriteriaId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int MaxMark { get; set; }
        public double Weight { get; set; }
        public int Order { get; set; }
    }
}
