using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Import
{
    public class ImportColumn
    {
        public string Name { get; set; }
        public InputColumn? SelectedInput { get; set; }
        public bool MatchExisting { get; set; }
        /// <summary>Should throw ParseException if string value is invalid.</summary>
        public Action<Applicant, string> ValueSetter { get; set; }

        public ImportColumn(string name, Action<Applicant, string> setter, bool match_existing = false)
        {
            Name = name;
            ValueSetter = setter;
            MatchExisting = match_existing;
        }
    }
}
