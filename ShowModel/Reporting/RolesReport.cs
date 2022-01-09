using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Reporting
{
    public class RolesReport : Report<(Item, Role)>
    {
        public const string DefaultReportType = "Roles";

        public override string ReportType => DefaultReportType;

        public RolesReport(CastGroup[] cast_groups, AlternativeCast[] alternative_casts)
            : base(AssignOrder(GenerateColumns(cast_groups, alternative_casts)))
        { }

        private static IEnumerable<Column<(Item, Role)>> GenerateColumns(CastGroup[] cast_groups, AlternativeCast[] alternative_casts)
        {
            // Item fields (duplicated from ItemsReport)
            yield return new Column<(Item Item, Role Role)>("Section & Item Names", p => string.Join(" - ", p.Item.Parents().Reverse().Skip(1).Select(n => n.Name).Concat(p.Item.Name.Yield()))) { Show = false }; // skip ShowRoot
            yield return new Column<(Item Item, Role Role)>("Item Name", p => p.Item.Name);
            foreach (var cast_group in cast_groups.InOrder())
                yield return new Column<(Item Item, Role Role)>($"Item Required {cast_group.Abbreviation}", p => p.Item.CountFor(cast_group)) { Show = false };
            yield return new Column<(Item Item, Role Role)>("Item Required Total", p => p.Item.CountByGroups.Sum(cbg => cbg.Count)) { Show = false };
            yield return new Column<(Item Item, Role Role)>("Item Section Depth", p => p.Item.Parents().Count() - 1) { Show = false }; // exclude ShowRoot
            yield return new Column<(Item Item, Role Role)>("Item Sections", p => string.Join(", ", p.Item.Parents().Reverse().Skip(1).Select(n => n.Name))) { Show = false }; // skip ShowRoot
            // Role fields
            yield return new Column<(Item Item, Role Role)>("Section, Item & Role Names", p => string.Join(" - ", p.Item.Parents().Reverse().Skip(1).Select(n => n.Name).Concat(p.Item.Name.Yield()).Concat(p.Role.Name.Yield()))) { Show = false }; // skip ShowRoot
            yield return new Column<(Item Item, Role Role)>("Item & Role Names", p => $"{p.Item.Name} - {p.Role.Name}") { Show = false };
            yield return new Column<(Item Item, Role Role)>("Role Name", p => p.Role.Name);
            foreach (var cast_group in cast_groups.InOrder())
                yield return new Column<(Item Item, Role Role)>($"Role Required {cast_group.Abbreviation}", p => p.Role.CountFor(cast_group));
            yield return new Column<(Item Item, Role Role)>("Role Required Total", p => p.Role.CountByGroups.Sum(cbg => cbg.Count));
            yield return new Column<(Item Item, Role Role)>("Requirement Count", p => p.Role.Requirements.Count) { Show = false };
            yield return new Column<(Item Item, Role Role)>("Requirements", p => string.Join(", ", p.Role.Requirements.Select(r => r.Name)));
            yield return new Column<(Item Item, Role Role)>("Casting Status", p => p.Role.CastingStatus(alternative_casts));
            //TODO cast counts by cast group and cast
            yield return new Column<(Item Item, Role Role)>("Cast Count", p => p.Role.Cast.Count);
            yield return new Column<(Item Item, Role Role)>("Cast List", p => string.Join(", ", p.Role.Cast.Select(a => $"#{a.CastNumberAndCast ?? "-"} {a.FirstName} {a.LastName}"))) { Show = false };
        }
    }
}
