using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Import
{
    /// <summary>
    /// Imports applicants from a CSV file
    /// </summary>
    public class CsvImporter
    {
        public InputColumn[] InputColumns { get; }
        public ImportColumn[] ImportColumns { get; }

        public CsvImporter(string file_name)
        {
            //TODO
            InputColumns = new InputColumn[]
            {
                new(0,"Col1"),
                new(1,"Col2"),
                new(2,"Col3"),
            };
            ImportColumns = new ImportColumn[]
            {
                new("Test1",(a,s) => { }),
                new("Test2",(a,s) => { }),
                new("Test3",(a,s) => { })
            };
        }

        public ImportResult Import(ICollection<Applicant> applicant_collection)
        {
            //TODO
            return default;
        }
    }

    public struct ImportResult
    {
        public int NewApplicantsAdded;
        public int ExistingApplicantsUpdated;
        public int ExistingApplicantsNotChanged;
    }
}
