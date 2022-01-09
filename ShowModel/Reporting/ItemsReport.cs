using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Reporting
{
    public class ItemsReport : Report<Item>
    {
        public override string ReportType => "Items";

        public ItemsReport(CastGroup[] cast_groups)
            : base(AssignOrder(GenerateColumns(cast_groups)))
        { }

        private static IEnumerable<Column<Item>> GenerateColumns(CastGroup[] cast_groups)
        {
            yield return new Column<Item>("Name", i => i.Name);
            foreach (var cast_group in cast_groups.InOrder())
            {
                var cg = cast_group;
                yield return new Column<Item>($"Required {cast_group.Name}", i => i.CountFor(cg));
            }
            yield return new Column<Item>("Required Total", i => i.CountByGroups.Sum(cbg => cbg.Count));
            yield return new Column<Item>("Section Depth", i => i.Parents().Count() - 1); // exclude ShowRoot
            yield return new Column<Item>("Sections", i => string.Join(", ", i.Parents().Reverse().Skip(1).Select(n => n.Name))); // skip ShowRoot
            yield return new Column<Item>("Role Count", i => i.Roles.Count);
            yield return new Column<Item>("Roles", i => string.Join(", ", i.Roles.Select(r => r.Name)));
            yield return new Column<Item>("Allowed Consecutives Count", i => i.AllowedConsecutives.Count);
            yield return new Column<Item>("Allowed Consecutives", i => string.Join("\n", i.AllowedConsecutives.Select(c => c.Description)));
        }
    }
}
