using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Criterias
{
    /// <summary>
    /// A criteria which is marked as one of a set of discreet options.
    /// This can also be a numeric scale in the order of the options.
    /// </summary>
    public class SelectCriteria : Criteria
    {
        internal static string[] DEFAULT_OPTIONS = new[] { "", "" };

        /// <summary>ValueConverter configured in <c cref="ShowContext.OnModelCreating">DbContext</c>.</summary>
        private string[] options;

        /// <summary>A list of options which are available for this criteria.
        /// NOTE: Changing this will not update the indicies which are already set as applicant ability marks.</summary>
        public string[] Options
        {
            get => options;
            set
            {
                if (value.Length < 2)
                    throw new ArgumentException("SelectCriteria.Options must contain at least 2 elements.");
                options = value;
                base.MaxMark = (uint)(options.Length - 1);
            }
        }

        public override uint MaxMark
        {
            set => throw new NotImplementedException("SelectCriteria.MaxMark cannot be set.");
        }

        public SelectCriteria()
        {
            options = DEFAULT_OPTIONS;
            base.MaxMark = (uint)(options.Length - 1);
        }
    }
}
