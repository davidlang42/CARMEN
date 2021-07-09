using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Criteria
{
    public class SelectCriteria : Criteria
    {
        private string[] options = new[] { "" };
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
            }
        }
        public override uint MaxMark
        {
            get => (uint)(Options.Length - 1);
            set => throw new NotImplementedException("SelectCriteria.MaxMark cannot be set."); //TODO confirm save/load still works
        }
    }
}
