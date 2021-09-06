using Microsoft.EntityFrameworkCore;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CarmenUI.ViewModels
{
    public class CastSummary : Summary
    {
        public override async Task LoadAsync(ShowContext c, CancellationToken cancel)
        {
            StartLoad();
            // cast groups
            var cast_groups = await c.CastGroups.Include(cg => cg.Members).ToArrayAsync();
            var alternative_cast_count = await c.AlternativeCasts.CountAsync();
            var sum = 0;
            foreach (var cast_group in cast_groups)
            {
                var count = cast_group.Members.Count;
                var row = new Row { Success = $"{count} in {cast_group.Name}" };
                var alternative_casts_for_group = cast_group.AlternateCasts ? alternative_cast_count : 1;
                var extra = count - cast_group.RequiredCount * alternative_casts_for_group;
                if (extra > 0)
                    row.Fail = $"({extra} too many)";
                else if (extra < 0)
                    row.Fail = $"({-extra} missing)";
                Rows.Add(row);
                sum += count;
            }
            Rows.Insert(0, new Row { Success = $"{sum} Cast Selected" });
            if (cast_groups.Any(cg => cg.AlternateCasts))
            {
                // same cast sets
                //TODO same cast sets (X same cast sets including Y applicants, error if any empty or single applicant)
                //TODO error if that they are not respected
                // alternative casts
                //TODO alternative casts
            }
            // tags
            var tags = await c.Tags.Include(cg => cg.Members).ToArrayAsync();
            foreach (var tag in tags)
            {
                var row = new Row { Success = $"{tag.Members.Count} tagged {tag.Name}" };
                if (tag.Members.GroupBy(a => a.CastGroup).Any(g => g.Key == null || (tag.CountFor(g.Key) is uint required_count && required_count != g.Count()))) //LATER does this need await?
                    row.Fail = $"(doesn't match required)";
                Rows.Add(row);
            }
            // cast number range and missing numbers
            int min_cast_num = 1;
            int cast_num = min_cast_num;
            var missing_nums = new List<int>();
            var incomplete_nums = new List<int>();
            foreach (var group in c.Applicants.Local.Where(a => a.IsAccepted).OrderBy(a => a.CastNumber).GroupBy(a => a.CastNumber))
            {
                if (group.Key == null)
                {
                    Rows.Add(new Row { Fail = $"{group.Count()} cast members without a cast number" });
                    continue;
                }
                while (cast_num != group.Key)
                    missing_nums.Add(cast_num++);
                var cast = group.ToArray();
                var expected_length = cast[0].CastGroup!.AlternateCasts ? alternative_cast_count : 1;
                if (cast.Length != expected_length)
                    incomplete_nums.Add(cast_num);
                cast_num++;
            }
            int max_cast_num = cast_num - 1;
            if (max_cast_num >= min_cast_num)
                Rows.Add(new Row {
                    Success = $"Cast numbers assigned #{min_cast_num} to #{max_cast_num}",
                    Fail = (missing_nums.Any(), incomplete_nums.Any()) switch
                    {
                        (true, true) => $"({missing_nums.Count} missing, {incomplete_nums.Count} incomplete)",
                        (true, false) => $"({missing_nums.Count} missing)",
                        (false, true) => $"({incomplete_nums.Count} incomplete)",
                        _ => null // (false, false)
                    }
                });
            FinishLoad(cancel, sum == 0);
        }
    }
}
