using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Reporting
{
    public class ApplicantReport : Report<Applicant>
    {
        public ApplicantReport()
            : base(GenerateColumns().ToArray())
        { }

        private static IEnumerable<Column<Applicant>> GenerateColumns()
        {
            yield return new Column<Applicant>("First Name", a => a.FirstName);
            yield return new Column<Applicant>("Last Name", a => a.LastName);
            yield return new Column<Applicant>("Gender", a => a.Gender);
            yield return new Column<Applicant>("Date of Birth", a => a.DateOfBirth, "dd/MM/yyyy");
            //foreach (var criteria in criterias)
            //    if (criteria is SelectCriteria select)
            //        yield return new ExportColumn(criteria.Name, a => a.Abilities.SingleOrDefault(ab => ab.Criteria == criteria) is Ability ab ? (ab.Mark > criteria.MaxMark ? ab.Mark.ToString() : select.Options[ab.Mark]) : "");
            //    else if (criteria is BooleanCriteria)
            //        yield return new ExportColumn(criteria.Name, a => a.Abilities.SingleOrDefault(ab => ab.Criteria == criteria) is Ability ab ? (ab.Mark > 1 ? ab.Mark.ToString() : (ab.Mark == 1 ? "Y" : "N")) : "");
            //    else if (criteria is NumericCriteria)
            //        yield return new ExportColumn(criteria.Name, a => a.Abilities.SingleOrDefault(ab => ab.Criteria == criteria)?.Mark.ToString() ?? "");
            //    else
            //        throw new ApplicationException($"Type not handled: {criteria.GetType().Name}");
            yield return new Column<Applicant>("Notes", a => a.Notes);
            yield return new Column<Applicant>("External Data", a => a.ExternalData);
            yield return new Column<Applicant>("Cast Group", a => a.CastGroup?.Name);
            yield return new Column<Applicant>("Cast Number", a => a.CastNumber);
            yield return new Column<Applicant>("Alternative Cast", a => a.AlternativeCast?.Initial);
            //foreach (var tag in tags)
            //    yield return new ExportColumn(tag.Name, a => a.Tags.Contains(tag) ? "Y" : "N");
        }
    }
}
