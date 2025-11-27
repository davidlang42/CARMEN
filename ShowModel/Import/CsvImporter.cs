using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
using CsvHelper;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Import
{
    /// <summary>
    /// Imports applicants from a CSV file
    /// </summary>
    public class CsvImporter : IDisposable
    {
        bool disposed;
        readonly CastGroup[] castGroups;
        readonly AlternativeCast[] alternativeCasts;
        readonly CsvReader csv;
        readonly string importPath; // with trailing backslash

        public InputColumn[] InputColumns { get; }
        public ImportColumn[] ImportColumns { get; }
        public DateTime MaximumDateOfBirth { get; set; } = DateTime.Now;

        public CsvImporter(string file_name, Criteria[] criterias, CastGroup[] cast_groups, AlternativeCast[] alternative_casts, Tag[] tags, Action<Image> delete_image, string current_user_name)
        {
            importPath = Path.GetDirectoryName(Path.GetFullPath(file_name)) ?? Directory.GetCurrentDirectory();
            if (!importPath.EndsWith(Path.DirectorySeparatorChar))
                importPath += Path.DirectorySeparatorChar;
            castGroups = cast_groups;
            alternativeCasts = alternative_casts;
            var stream = UserException.Handle(() => new StreamReader(file_name), "Error opening CSV file.");
            try
            {
                csv = new CsvReader(stream, CultureInfo.InvariantCulture);
                csv.Read();
                csv.ReadHeader();
            }
            catch (Exception ex)
            {
                throw new UserException(ex, $"Error reading CSV file.");
            }
            if (csv.HeaderRecord is not string[] header) {
                throw new UserException("Failed to read header");
            }
            InputColumns = csv.HeaderRecord.Select((h, i) => new InputColumn(i, h)).ToArray();
            ImportColumns = GenerateColumns(criterias, tags, delete_image, current_user_name).ToArray();
            LoadColumnMap(InputColumns.ToDictionary(c => c.Header, new FilteredStringComparer(char.IsLetterOrDigit, StringComparison.InvariantCultureIgnoreCase)));
        }

        public ImportResult Import(ICollection<Applicant> applicant_collection, ShowRoot show_root, Action<int>? progress_callback = null)
        {
            if (disposed)
                throw new InvalidOperationException("Tried to use after dispose");
            var result = new ImportResult {
                ParseErrors = new()
            };
            var match_columns = ImportColumns.Where(c => c.MatchExisting && c.SelectedInput != null).ToArray();
            while (csv.Read())
            {
                Applicant[] applicants;
                if (match_columns.Length > 0)
                {
                    IEnumerable<Applicant> existing = applicant_collection;
                    foreach (var column in ImportColumns.Where(c => c.MatchExisting && c.SelectedInput != null))
                        existing = existing.Where(a => column.ValueComparer(a, csv.GetField(column.SelectedInput!.Index) ?? throw new Exception("Field not found.")));
                    applicants = existing.ToArray();
                }
                else
                    applicants = Array.Empty<Applicant>();
                if (applicants.Length == 0)
                {
                    // new applicant
                    var applicant = new Applicant { ShowRoot = show_root };
                    foreach (var column in ImportColumns.Where(c => c.SelectedInput != null))
                        try
                        {
                            column.ValueSetter(applicant, csv.GetField(column.SelectedInput!.Index) ?? throw new Exception("Field not found."));
                        }
                        catch (ParseException ex)
                        {
                            Log.Error(ex, $"{nameof(CsvImporter)}.{nameof(Import)} row {result.RecordsProcessed + 1}");
                            result.ParseErrors.Add((result.RecordsProcessed + 1, ex.Message));
                        }
                    applicant_collection.Add(applicant);
                    result.NewApplicantsAdded++;
                }
                else if (applicants.Length == 1)
                {
                    // matched applicant
                    bool changed = false;
                    foreach (var column in ImportColumns.Where(c => c.SelectedInput != null))
                    {
                        var value = csv.GetField(column.SelectedInput!.Index) ?? throw new Exception("Field not found.");
                        if (!column.ValueComparer(applicants[0], value))
                        {
                            try
                            {
                                column.ValueSetter(applicants[0], value);
                                changed = true;
                            }
                            catch (ParseException ex)
                            {
                                Log.Error(ex, $"{nameof(CsvImporter)}.{nameof(Import)} row {result.RecordsProcessed + 1}");
                                result.ParseErrors.Add((result.RecordsProcessed + 1, ex.Message));
                            }
                        }
                    }
                    if (changed)
                        result.ExistingApplicantsUpdated++;
                    else
                        result.ExistingApplicantsNotChanged++;
                }
                else
                {
                    // more than 1 match
                    result.ParseErrors.Add((result.RecordsProcessed + 1, $"Matched {applicants.Length.Plural("row")} by columns {string.Join(", ", ImportColumns.Where(c => c.MatchExisting).Select(c => $"'{c.Name}'"))}"));
                }
                result.RecordsProcessed++;
                progress_callback?.Invoke(result.RecordsProcessed);
            }
            return result;
        }

        public void LoadColumnMap(Dictionary<string, InputColumn> field_name_to_input_column)
        {
            foreach (var column in ImportColumns)
                if (field_name_to_input_column.TryGetValue(column.Name, out var input))
                    column.SelectedInput = input;
        }

        private IEnumerable<ImportColumn> GenerateColumns(Criteria[] criterias, Tag[] tags, Action<Image> delete_image, string current_user_name)
        {
            void set_photo(Applicant a, Image v) {
                if (a.Photo != null)
                    delete_image(a.Photo);
                a.Photo = v;
            }
            yield return ImportColumn.ForString("First Name", a => a.FirstName, (a, s) => a.FirstName = s);
            yield return ImportColumn.ForString("Last Name", a => a.LastName, (a, s) => a.LastName = s);
            yield return ImportColumn.ForNullable("Gender", a => a.Gender, (a, v) => a.Gender = v, ParseGender);
            yield return ImportColumn.ForNullable("Date of Birth", a => a.DateOfBirth, (a, v) => a.DateOfBirth = v, ParseDateOfBirth);
            foreach (var criteria in criterias.InOrder())
                yield return ImportColumn.ForNullable(criteria.Name, a => a.Abilities.SingleOrDefault(ab => ab.Criteria == criteria)?.Mark, (a, v) => a.SetMarkFor(criteria, v), s => ParseCriteriaMark(criteria, s));
            yield return new ImageImportColumn("Photo", set_photo, LoadImageData);
            yield return new NoteImportColumn("Add Note", a => a.Notes, current_user_name);
            yield return ImportColumn.ForString("External Data", a => a.ExternalData, (a, v) => a.ExternalData = v, false);
            // everything below here should only be imported if they are already accepted into the cast
            yield return ImportColumn.ForNullable("Cast Group", a => a.CastGroup, (a, v) => a.CastGroup = v, ParseCastGroup);
            yield return ImportColumn.ForConditionalNullable("Cast Number", a => a.CastNumber, (a, v) => a.CastNumber = v, ParseCastNumber);
            yield return ImportColumn.ForConditionalNullable("Alternative Cast", a => a.AlternativeCast, (a, v) => a.AlternativeCast = v, ParseAlternativeCast);
            foreach (var tag in tags.InNameOrder())
                yield return ImportColumn.ForConditionalNullable(tag.Name, a => a.Tags.Contains(tag), (a, v) => SetTag(a, tag, v), (a, s) => ParseTagFlag(a, tag, s));
        }

        private byte[] LoadImageData(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ApplicationException("Tried to load image with an empty filename");
            var fullpath = importPath + filename;
            if (!File.Exists(fullpath))
                throw new ParseException("image", filename, $"a file within the same folder ({importPath})");
            try
            {
                return File.ReadAllBytes(fullpath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(CsvImporter)}.{nameof(LoadImageData)}");
                throw new ParseException("image", filename, $"no error reading file ({ex.Message})");
            }
        }

        private Gender? ParseGender(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            return value.ToLower() switch
            {
                "m" => Gender.Male,
                "male" => Gender.Male,
                "f" => Gender.Female,
                "female" => Gender.Female,
                _ => throw new ParseException("gender", value, "M/F/Male/Female")
            };
        }

        private DateTime? ParseDateOfBirth(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            if (DateTime.TryParse(value, out var date))
            {
                if (date > MaximumDateOfBirth)
                    throw new ParseException("date of birth", value, $"a date before {MaximumDateOfBirth}");
                return date;
            }
            throw new ParseException("date of birth", value, "dd/mm/yyyy");
        }

        private static uint? ParseCriteriaMark(Criteria criteria, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            if (uint.TryParse(value, out var mark))
            {
                if (mark > criteria.MaxMark)
                    throw new ParseException(criteria.Name, value, $"a number between 0 and {criteria.MaxMark}");
                return mark;
            }
            string expected;
            var lower = value.ToLower();
            if (criteria is BooleanCriteria)
            {
                if (lower == "n" || lower == "no")
                    return 0;
                if (lower == "y" || lower == "yes")
                    return 1;
                expected = "Y/N/Yes/No";
            }
            else if (criteria is SelectCriteria select)
            {
                for (var i = 0; i < select.Options.Length; i++)
                    if (value.Equals(select.Options[i], StringComparison.InvariantCultureIgnoreCase))
                        return (uint)i;
                expected = string.Join(", ", select.Options);
            }
            else if (criteria is NumericCriteria)
                expected = $"a whole number between 0 and {criteria.MaxMark}";
            else
                throw new ApplicationException($"Type not handled: {criteria.GetType().Name}");
            throw new ParseException(criteria.Name, value, expected);
        }

        private CastGroup? ParseCastGroup(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            foreach (var cast_group in castGroups)
                if (value.Equals(cast_group.Name, StringComparison.InvariantCultureIgnoreCase)
                    || value.Equals(cast_group.Abbreviation, StringComparison.InvariantCultureIgnoreCase))
                    return cast_group;
            throw new ParseException("cast group", value, string.Join(", ", castGroups.Select(cg => cg.Name)));
        }

        private int? ParseCastNumber(Applicant applicant, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            if (!applicant.IsAccepted)
                throw new ParseException("cast number", value, "no cast number applicant is accepted");
            if (int.TryParse(value, out var number))
                return number;
            throw new ParseException("cast number", value, "a whole number only");
        }

        private AlternativeCast? ParseAlternativeCast(Applicant applicant, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            if (applicant.CastGroup == null)
                throw new ParseException("alternative cast", value, "no alternative cast unless applicant is accepted");
            if (!applicant.CastGroup.AlternateCasts)
                throw new ParseException("alternative cast", value, "no alternative cast if cast group does not alternate");
            foreach (var alternative_cast in alternativeCasts)
                if (value.Equals(alternative_cast.Name, StringComparison.InvariantCultureIgnoreCase)
                    || value.Equals(alternative_cast.Initial.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    return alternative_cast;
            throw new ParseException("alternative cast", value, string.Join(", ", alternativeCasts.Select(cg => cg.Name)));
        }

        public static bool? ParseTagFlag(Applicant applicant, Tag tag, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            var apply_tag = value.ToLower() switch
            {
                "y" => true,
                "n" => false,
                "yes" => true,
                "no" => false,
                _ => throw new ParseException(tag.Name, value, "Y/N/Yes/No")
            };
            if (apply_tag && !applicant.IsAccepted)
                throw new ParseException(tag.Name, value, "no tag to be applied unless applicant is accepted");
            return apply_tag;
        }

        public static void SetTag(Applicant applicant, Tag tag, bool? value)
        {
            if (value == null)
                return; // nothing to do
            if (value.Value && !applicant.Tags.Contains(tag))
            {
                applicant.Tags.Add(tag);
                tag.Members.Add(applicant);
            }
            else if (!value.Value && applicant.Tags.Contains(tag))
            {
                applicant.Tags.Remove(tag);
                tag.Members.Remove(applicant);
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                csv.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }

    public struct ImportResult
    {
        public int RecordsProcessed;
        public int NewApplicantsAdded;
        public int ExistingApplicantsUpdated;
        public int ExistingApplicantsNotChanged;
        public List<(int, string)> ParseErrors;
    }
}
