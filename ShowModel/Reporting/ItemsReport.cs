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
        public const string DefaultReportType = "Items";

        public override string ReportType => DefaultReportType;

        public ItemsReport(List<int> item_ids_in_order, CastGroup[] cast_groups)
            : base(AssignOrder(GenerateColumns(item_ids_in_order, cast_groups)))
        { }

        private static IEnumerable<Column<Item>> GenerateColumns(List<int> item_ids_in_order, CastGroup[] cast_groups)
        {
            yield return new Column<Item>("Show Order", i => item_ids_in_order.IndexOf(i.NodeId)) { Show = false };
            yield return new Column<Item>("Section & Item Names", i => string.Join(" - ", i.Parents().Reverse().Skip(1).Select(n => n.Name).Concat(i.Name.Yield()))) { Show = false }; // skip ShowRoot
            yield return new Column<Item>("Name", i => i.Name);
            foreach (var cast_group in cast_groups.InOrder())
                yield return new Column<Item>($"Required {cast_group.Abbreviation}", i => i.CountFor(cast_group));
            yield return new Column<Item>("Required Total", i => i.CountByGroups.Sum(cbg => cbg.Count));
            yield return new Column<Item>("Section Depth", i => i.Parents().Count() - 1) { Show = false }; // exclude ShowRoot
            yield return new Column<Item>("Sections", i => string.Join(", ", i.Parents().Reverse().Skip(1).Select(n => n.Name))); // skip ShowRoot
            yield return new Column<Item>("Role Count", i => i.Roles.Count);
            yield return new Column<Item>("Roles", i => string.Join(", ", i.Roles.Select(r => r.Name))) { Show = false };
            yield return new Column<Item>("Allowed Consecutives Count", i => i.AllowedConsecutives.Count) { Show = false };
            yield return new Column<Item>("Allowed Consecutives", i => string.Join("\n", i.AllowedConsecutives.Select(c => c.Description))) { Show = false };
        }
    }
}
