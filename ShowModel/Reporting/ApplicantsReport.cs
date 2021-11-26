using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Reporting
{
    public class ApplicantsReport : Report<Applicant>
    {
        public override string ReportType => "All Applicants";

        public ApplicantsReport(Criteria[] criterias, Tag[] tags)
            : base(AssignOrder(GenerateColumns(criterias, tags)))
        { }

        private static IEnumerable<Column<Applicant>> GenerateColumns(Criteria[] criterias, Tag[] tags)
        {
            yield return new Column<Applicant>("First Name", a => a.FirstName);
            yield return new Column<Applicant>("Last Name", a => a.LastName);
            yield return new Column<Applicant>("Full Name", a => $"{a.LastName}, {a.FirstName}") { Show = false };
            yield return new Column<Applicant>("Gender", a => a.Gender);
            yield return new Column<Applicant>("Date of Birth", a => a.DateOfBirth, "dd/MM/yyyy");
            yield return new Column<Applicant>("Age", a => a.Age, "0 yrs");
            foreach (var criteria in criterias.InOrder())
                if (criteria is SelectCriteria select)
                    yield return new Column<Applicant>(criteria.Name, a => a.Abilities.SingleOrDefault(ab => ab.Criteria.CriteriaId == criteria.CriteriaId) is Ability ab ? (ab.Mark > criteria.MaxMark ? ab.Mark : select.Options[ab.Mark]) : null);
                else if (criteria is BooleanCriteria)
                    yield return new Column<Applicant>(criteria.Name, a => a.Abilities.SingleOrDefault(ab => ab.Criteria.CriteriaId == criteria.CriteriaId) is Ability ab ? (ab.Mark > 1 ? ab.Mark : ab.Mark == 1) : null);
                else if (criteria is NumericCriteria)
                    yield return new Column<Applicant>(criteria.Name, a => a.Abilities.SingleOrDefault(ab => ab.Criteria.CriteriaId == criteria.CriteriaId)?.Mark);
                else
                    throw new ApplicationException($"Type not handled: {criteria.GetType().Name}");
            yield return new Column<Applicant>("Overall Ability", a => OverallAbility(a, criterias));
            yield return new Column<Applicant>("Notes", a => a.Notes);
            yield return new Column<Applicant>("External Data", a => a.ExternalData);
            yield return new Column<Applicant>("Cast Group", a => a.CastGroup?.Name);
            yield return new Column<Applicant>("Cast Number", a => a.CastNumber);
            yield return new Column<Applicant>("Alternative Cast", a => a.AlternativeCast?.Initial);
            yield return new Column<Applicant>("Cast Number And Cast", a => $"{a.CastNumber}{a.AlternativeCast?.Initial}") { Show = false };
            yield return new Column<Applicant>("Cast Group And Cast", a => $"{a.CastGroup?.Name}{(a.AlternativeCast == null ? "" : $" ({a.AlternativeCast.Name})")}") { Show = false };
            foreach (var tag in tags.InNameOrder())
            {
                yield return new Column<Applicant>(tag.Name, a => a.Tags.Any(t => t.TagId == tag.TagId));
                yield return new Column<Applicant>($"{tag.Name} (Name/Blank)", a => a.Tags.Any(t => t.TagId == tag.TagId) ? tag.Name : "") { Show = false };
            }
        }

        /// <summary>Same calculation as WeightedSumEngine, except returns null for incomplete applicants</summary>
        private static int? OverallAbility(Applicant applicant, Criteria[] criterias)
        {
            if (!applicant.HasAuditioned(criterias))
                return null;
            return Convert.ToInt32(applicant.Abilities.Sum(a => (double)a.Mark / a.Criteria.MaxMark * a.Criteria.Weight));    
        }

        public async Task<int> ExportPhotos(string file_name, Applicant[] applicants, Action<int>? start_each_applicant_callback) // mostly copied from Report<T>.ExportCsv(), Group(), SetData()
        {
            var ordered_columns = Columns.Where(c => c.Show).InOrder().ToArray();
            if (ordered_columns.Length == 0 || applicants.Length == 0)
                return 0; // nothing to export
            int count = 0;
            using (var zip = ZipFile.Open(file_name, ZipArchiveMode.Create)) //TODO handle file io exceptions
            {
                //TODO make new folders per group
                //if (GroupColumn != null)
                //    rows = Group(rows, Rows[0].Length, IndexOf(GroupColumn), IndexOf(ordered_columns.First()));
                //object? previous_group = null;
                //foreach (var row in rows.OrderBy(r => r[group_column_index]))
                //{
                //    if (!Equals(row[group_column_index], previous_group))
                //    {
                //        var group_header_row = new object?[row_length];
                //        previous_group = group_header_row[first_column_index] = row[group_column_index];
                //        yield return group_header_row;
                //    }
                //    yield return row;
                //}
                for (var i = 0; i < applicants.Length; i++)
                {
                    start_each_applicant_callback?.Invoke(i);
                    if (await Task.Run(() => applicants[i].Photo) is Image image)
                    {
                        var extension = Path.GetExtension(image.Name);
                        if (string.IsNullOrWhiteSpace(extension))
                            extension = ".BMP";
                        var field_values = ordered_columns.Select(c => FormatAsString(c.ValueGetter(applicants[i])));
                        var filename = string.Join("-", field_values) + extension;
                        var sanitised_filename = string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
                        var entry = zip.CreateEntry(sanitised_filename); //TODO handle duplicate file names
                        using (var stream = entry.Open())
                            await stream.WriteAsync(image.ImageData, 0, image.ImageData.Length);
                        count++;
                    }
                }
            }
            return count;
        }
    }
}
