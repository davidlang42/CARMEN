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
        //TODO differentiate numeric criteria from "select from list" criteria, maybe also bool crit?
        [Key]
        public int CriteriaId { get; private set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        [Range(1, uint.MaxValue)]
        public uint MaxMark { get; set; }
        public double Weight { get; set; }
        public int Order { get; set; }
    }
}
