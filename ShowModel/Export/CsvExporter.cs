using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Export
{
    public class CsvExporter
    {
        public ExportColumn[] ExportColumns { get; }

        public CsvExporter(Criteria[] criterias, Tag[] tags)
        {
            ExportColumns = GenerateColumns(criterias, tags).ToArray();
        }

        public int Export(string file_name, IEnumerable<Applicant> applicants)
        {
            int count = 0;
            using (var writer = new StreamWriter(file_name)) //TODO handle file io exceptions
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (var column in ExportColumns)
                    csv.WriteField(column.Name);
                csv.NextRecord();
                foreach (var applicant in applicants)
                {
                    foreach (var column in ExportColumns)
                        csv.WriteField(column.ValueGetter(applicant));
                    csv.NextRecord();
                    count++;
                }
            }
            return count;
        }

        private IEnumerable<ExportColumn> GenerateColumns(Criteria[] criterias, Tag[] tags)
        {
            yield return new ExportColumn("First Name", a => a.FirstName);
            yield return new ExportColumn("Last Name", a => a.LastName);
            yield return new ExportColumn("Gender", a => a.Gender?.ToString() ?? "");
            yield return new ExportColumn("Date of Birth", a => $"{a.DateOfBirth:dd/MM/yyyy}");
            foreach (var criteria in criterias.InOrder())
                if (criteria is SelectCriteria select)
                    yield return new ExportColumn(criteria.Name, a => a.Abilities.SingleOrDefault(ab => ab.Criteria == criteria) is Ability ab ? (ab.Mark > criteria.MaxMark ? ab.Mark.ToString() : select.Options[ab.Mark]) : "");
                else if (criteria is BooleanCriteria)
                    yield return new ExportColumn(criteria.Name, a => a.Abilities.SingleOrDefault(ab => ab.Criteria == criteria) is Ability ab ? (ab.Mark > 1 ? ab.Mark.ToString() : (ab.Mark == 1 ? "Y" : "N")) : "");
                else if (criteria is NumericCriteria)
                    yield return new ExportColumn(criteria.Name, a => a.Abilities.SingleOrDefault(ab => ab.Criteria == criteria)?.Mark.ToString() ?? "");
                else
                    throw new ApplicationException($"Type not handled: {criteria.GetType().Name}");
            yield return new ExportColumn("Notes", a => a.Notes);
            yield return new ExportColumn("External Data", a => a.ExternalData);
            yield return new ExportColumn("Cast Group", a => a.CastGroup?.Name ?? "");
            yield return new ExportColumn("Cast Number", a => a.CastNumber?.ToString() ?? "");
            yield return new ExportColumn("Alternative Cast", a => a.AlternativeCast?.Initial.ToString() ?? "");
            foreach (var tag in tags.InNameOrder())
                yield return new ExportColumn(tag.Name, a => a.Tags.Contains(tag) ? "Y" : "N");
        }
    }
}
