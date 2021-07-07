using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// The details of the current show
    /// </summary>
    public class ShowSettings
    {
        /// <summary>Primary Key configured in <c cref="ShowContext.OnModelCreating">DbContext</c>.</summary>
        private int _id;
        public string Name { get; set; } = "";
        public DateTime? ShowDate { get; set; }
        public Image? Logo { get; set; }
    }
}
