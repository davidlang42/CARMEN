using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Export
{
    public class ExportColumn
    {
        public string Name { get; }
        public Func<Applicant, string> ValueGetter { get; }
        
        public ExportColumn(string name, Func<Applicant, string> getter)
        {
            Name = name;
            ValueGetter = getter;
        }
    }
}
