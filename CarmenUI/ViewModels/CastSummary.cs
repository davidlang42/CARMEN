using ShowModel;
using ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class CastSummary : Summary
    {
        public void Load(IReadOnlyCollection<CastGroup> cast_groups, IReadOnlyCollection<Tag> tags)
        {
            StartLoad();
            var sum = 0;
            foreach (var cast_group in cast_groups)
            {
                var count = cast_group.Members.Count;
                var row = new Row { Success = $"{count} in {cast_group.Name}" };
                var extra = count - cast_group.RequiredCount;
                if (extra > 0)
                    row.Fail = $"({extra} too many)";
                else if (extra < 0)
                    row.Fail = $"({-extra} missing)";
                Rows.Add(row);
                sum += count;
            }
            Rows.Insert(0, new Row { Success = $"{sum} Cast Selected" });
            foreach (var tag in tags)
            {
                var count = tag.Members.Count;
                var row = new Row { Success = $"{count} tagged {tag.Name}" };
                if (tag.Members.GroupBy(a => a.CastGroup).Any(g => g.Key == null || tag.CountFor(g.Key) is not uint required_count || required_count != g.Count()))
                    row.Fail = $"(doesn't match required)";
                Rows.Add(row);
            }
            FinishLoad(true);
        }
    }
}
