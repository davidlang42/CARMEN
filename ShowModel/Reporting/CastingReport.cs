using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Reporting
{
    public class CastingReport : Report<(Item, Role, Applicant)>
    {
        public override string ReportType => "Casting";

        public CastingReport(CastGroup[] cast_groups, AlternativeCast[] alternative_casts, Criteria[] criterias, Tag[] tags)
            : base(AssignOrder(GenerateColumns(cast_groups, alternative_casts, criterias, tags)))
        { }

        private static IEnumerable<Column<(Item, Role, Applicant)>> GenerateColumns(CastGroup[] cast_groups, AlternativeCast[] alternative_casts, Criteria[] criterias, Tag[] tags)
        {
            // Item fields (duplicated from ItemsReport)
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Section & Item Names", p => string.Join(" - ", p.Item.Parents().Reverse().Skip(1).Select(n => n.Name).Concat(p.Item.Name.Yield()))) { Show = false }; // skip ShowRoot
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Item Name", p => p.Item.Name);
            foreach (var cast_group in cast_groups.InOrder())
                yield return new Column<(Item Item, Role Role, Applicant Applicant)>($"Item Required {cast_group.Name}", p => p.Item.CountFor(cast_group)) { Show = false };
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Item Required Total", p => p.Item.CountByGroups.Sum(cbg => cbg.Count)) { Show = false };
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Item Section Depth", p => p.Item.Parents().Count() - 1) { Show = false }; // exclude ShowRoot
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Item Sections", p => string.Join(", ", p.Item.Parents().Reverse().Skip(1).Select(n => n.Name))) { Show = false }; // skip ShowRoot

            // Role fields (duplicated from RolesReport)
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Section, Item & Role Names", p => string.Join(" - ", p.Item.Parents().Reverse().Skip(1).Select(n => n.Name).Concat(p.Item.Name.Yield()).Concat(p.Role.Name.Yield()))) { Show = false }; // skip ShowRoot
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Item & Role Names", p => $"{p.Item.Name} - {p.Role.Name}") { Show = false };
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Role Name", p => p.Role.Name);
            foreach (var cast_group in cast_groups.InOrder())
                yield return new Column<(Item Item, Role Role, Applicant Applicant)>($"Role Required {cast_group.Name}", p => p.Role.CountFor(cast_group)) { Show = false };
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Role Required Total", p => p.Role.CountByGroups.Sum(cbg => cbg.Count)) { Show = false };
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Requirement Count", p => p.Role.Requirements.Count) { Show = false };
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Requirements", p => string.Join(", ", p.Role.Requirements.Select(r => r.Name)));
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Casting Status", p => p.Role.CastingStatus(alternative_casts)) { Show = false };

            // Applicant fields (duplicated from ApplicantsReport)
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("First Name", p => p.Applicant.FirstName);
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Last Name", p => p.Applicant.LastName);
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Full Name", p => $"{p.Applicant.LastName}, {p.Applicant.FirstName}") { Show = false };
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Gender", p => p.Applicant.Gender) { Show = false };
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Date of Birth", p => p.Applicant.DateOfBirth, "dd/MM/yyyy") { Show = false };
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Age", p => p.Applicant.Age, "0 yrs") { Show = false };
            foreach (var criteria in criterias.InOrder())
                if (criteria is SelectCriteria select)
                    yield return new Column<(Item Item, Role Role, Applicant Applicant)>(criteria.Name, p => p.Applicant.Abilities.SingleOrDefault(ab => ab.Criteria.CriteriaId == criteria.CriteriaId) is Ability ab ? (ab.Mark > criteria.MaxMark ? ab.Mark : select.Options[ab.Mark]) : null) { Show = false };
                else if (criteria is BooleanCriteria)
                    yield return new Column<(Item Item, Role Role, Applicant Applicant)>(criteria.Name, p => p.Applicant.Abilities.SingleOrDefault(ab => ab.Criteria.CriteriaId == criteria.CriteriaId) is Ability ab ? (ab.Mark > 1 ? ab.Mark : ab.Mark == 1) : null) { Show = false };
                else if (criteria is NumericCriteria)
                    yield return new Column<(Item Item, Role Role, Applicant Applicant)>(criteria.Name, p => p.Applicant.Abilities.SingleOrDefault(ab => ab.Criteria.CriteriaId == criteria.CriteriaId)?.Mark) { Show = false };
                else
                    throw new ApplicationException($"Type not handled: {criteria.GetType().Name}");
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Overall Ability", p => ApplicantsReport.OverallAbility(p.Applicant, criterias)) { Show = false };
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("External Data", p => p.Applicant.ExternalData);
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Cast Group", p => p.Applicant.CastGroup?.Name);
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Cast Number", p => p.Applicant.CastNumber);
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Alternative Cast", p => p.Applicant.AlternativeCast?.Initial);
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Cast Number And Cast", p => $"{p.Applicant.CastNumber}{p.Applicant.AlternativeCast?.Initial}") { Show = false };
            yield return new Column<(Item Item, Role Role, Applicant Applicant)>("Cast Group And Cast", p => $"{p.Applicant.CastGroup?.Name}{(p.Applicant.AlternativeCast == null ? "" : $" ({p.Applicant.AlternativeCast.Name})")}") { Show = false };
            foreach (var tag in tags.InNameOrder())
            {
                yield return new Column<(Item Item, Role Role, Applicant Applicant)>(tag.Name, p => p.Applicant.Tags.Any(t => t.TagId == tag.TagId));
                yield return new Column<(Item Item, Role Role, Applicant Applicant)>($"{tag.Name} (Name/Blank)", p => p.Applicant.Tags.Any(t => t.TagId == tag.TagId) ? tag.Name : "") { Show = false };
            }
        }
    }
}
