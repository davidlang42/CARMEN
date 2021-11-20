using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
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
        public List<string> Errors { get; } = new();
        public DateTime MaximumDateOfBirth { get; set; } = DateTime.Now; //TODO set this to show's date

        private CastGroup[] castGroups;
        private AlternativeCast[] alternativeCasts;

        public CsvImporter(string file_name, Criteria[] criterias, CastGroup[] cast_groups, AlternativeCast[] alternative_casts, Tag[] tags)
        {
            castGroups = cast_groups;
            alternativeCasts = alternative_casts;
            //TODO
            InputColumns = new InputColumn[]
            {
                new(0,"Col1"),
                new(1,"Col2"),
                new(2,"Col3"),
            };
            ImportColumns = GenerateColumns(criterias, tags).ToArray();
        }

        public ImportResult Import(ICollection<Applicant> applicant_collection)
        {
            //TODO
            return default;
        }

        private IEnumerable<ImportColumn> GenerateColumns(Criteria[] criterias, Tag[] tags)
        {
            yield return new("First Name", (a, s) => a.FirstName = s);
            yield return new("Last Name", (a, s) => a.LastName = s);
            yield return new("Gender", (a, s) => a.Gender = ParseGender(s));
            yield return new("Date of Birth", (a, s) => a.DateOfBirth = ParseDateOfBirth(s));
            foreach (var criteria in criterias)
                yield return new(criteria.Name, (a, s) => a.SetMarkFor(criteria, ParseCriteriaMark(criteria, s)));
            yield return new("Notes", (a, s) => a.Notes = s);
            yield return new("External Data", (a, s) => a.ExternalData = s);
            // everything below here should only be imported if they are already accepted into the cast
            yield return new("Cast Group", (a, s) => a.CastGroup = ParseCastGroup(s));
            yield return new("Cast Number", (a, s) => a.CastNumber = ParseCastNumber(a.IsAccepted, s));
            yield return new("Alternative Cast", (a, s) => a.AlternativeCast = ParseAlternativeCast(a.CastGroup, s));
            foreach (var tag in tags)
                yield return new(tag.Name, (a, s) => SetTag(a, tag, s));
        }

        private Gender ParseGender(string value)
            => value.ToLower() switch
            {
                "m" => Gender.Male,
                "male" => Gender.Male,
                "f" => Gender.Female,
                "female" => Gender.Female,
                _ => throw new ParseException("gender", value, "M/F/Male/Female")
            };

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
            throw new ParseException("date of birth", value); //TODO this should suggest the expected format
        }

        private uint? ParseCriteriaMark(Criteria criteria, string value)
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

        private int? ParseCastNumber(bool is_accepted, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            if (!is_accepted)
                throw new ParseException("cast number", value, "no cast number applicant is accepted");
            if (int.TryParse(value, out var number))
                return number;
            throw new ParseException("cast number", value, "a whole number only");
        }

        private AlternativeCast? ParseAlternativeCast(CastGroup? cast_group, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            if (cast_group == null)
                throw new ParseException("alternative cast", value, "no alternative cast unless applicant is accepted");
            if (!cast_group.AlternateCasts)
                throw new ParseException("alternative cast", value, "no alternative cast if cast group does not alternate");
            foreach (var alternative_cast in alternativeCasts)
                if (value.Equals(alternative_cast.Name, StringComparison.InvariantCultureIgnoreCase)
                    || value.Equals(alternative_cast.Initial.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    return alternative_cast;
            throw new ParseException("alternative cast", value, string.Join(", ", alternativeCasts.Select(cg => cg.Name)));
        }

        public void SetTag(Applicant applicant, Tag tag, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return; // nothing to do
            bool apply_tag = value.ToLower() switch
            {
                "y" => true,
                "n" => false,
                "yes" => true,
                "no" => false,
                _ => throw new ParseException(tag.Name, value, "Y/N/Yes/No")
            };
            if (apply_tag && !applicant.IsAccepted)
                throw new ParseException(tag.Name, value, "no tag to be applied unless applicant is accepted");
            if (apply_tag && !applicant.Tags.Contains(tag))
            {
                applicant.Tags.Add(tag);
                tag.Members.Add(applicant);
            }
            else if (!apply_tag && applicant.Tags.Contains(tag))
            {
                applicant.Tags.Remove(tag);
                tag.Members.Remove(applicant);
            }
        }
    }

    public struct ImportResult
    {
        public int NewApplicantsAdded;
        public int ExistingApplicantsUpdated;
        public int ExistingApplicantsNotChanged;
    }
}
