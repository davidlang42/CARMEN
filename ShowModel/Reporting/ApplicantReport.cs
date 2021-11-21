using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Reporting
{
    public class ApplicantReport : Report<Applicant>
    {
        public ApplicantReport(Criteria[] criterias, Tag[] tags)
            : base(AssignOrder(GenerateColumns(criterias, tags)))
        { }

        private static IEnumerable<Column<Applicant>> GenerateColumns(Criteria[] criterias, Tag[] tags)
        {
            yield return new Column<Applicant>("First Name", a => a.FirstName);
            yield return new Column<Applicant>("Last Name", a => a.LastName);
            yield return new Column<Applicant>("Gender", a => a.Gender);
            yield return new Column<Applicant>("Date of Birth", a => a.DateOfBirth, "dd/MM/yyyy");
            foreach (var criteria in criterias.InOrder())
                if (criteria is SelectCriteria select)
                    yield return new Column<Applicant>(criteria.Name, a => a.Abilities.SingleOrDefault(ab => ab.Criteria == criteria) is Ability ab ? (ab.Mark > criteria.MaxMark ? ab.Mark : select.Options[ab.Mark]) : null);
                else if (criteria is BooleanCriteria)
                    yield return new Column<Applicant>(criteria.Name, a => a.Abilities.SingleOrDefault(ab => ab.Criteria == criteria) is Ability ab ? (ab.Mark > 1 ? ab.Mark : ab.Mark == 1) : null);
                else if (criteria is NumericCriteria)
                    yield return new Column<Applicant>(criteria.Name, a => a.Abilities.SingleOrDefault(ab => ab.Criteria == criteria)?.Mark);
                else
                    throw new ApplicationException($"Type not handled: {criteria.GetType().Name}");
            yield return new Column<Applicant>("Notes", a => a.Notes);
            yield return new Column<Applicant>("External Data", a => a.ExternalData);
            yield return new Column<Applicant>("Cast Group", a => a.CastGroup?.Name);
            yield return new Column<Applicant>("Cast Number", a => a.CastNumber);
            yield return new Column<Applicant>("Alternative Cast", a => a.AlternativeCast?.Initial);
            foreach (var tag in tags.InNameOrder())
                yield return new Column<Applicant>(tag.Name, a => a.Tags.Contains(tag));
        }
    }
}
