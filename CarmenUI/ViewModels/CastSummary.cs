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
            // cast groups / alternative casts
            var cast_groups = await c.CastGroups.Include(cg => cg.Members).ToArrayAsync();
            var alternative_casts = await c.AlternativeCasts.ToArrayAsync();
            var sum = 0;
            foreach (var cast_group in cast_groups)
            {
                var count = cast_group.Members.Count;
                var row = new Row { Success = $"{count} in {cast_group.Name}" };
                var alternative_casts_for_group = cast_group.AlternateCasts ? alternative_casts.Length : 1;
                if (cast_group.RequiredCount is uint required)
                {
                    var extra = count - required * alternative_casts_for_group;
                    if (extra > 0)
                        row.Fail = $"({extra} too many)";
                    else if (extra < 0)
                        row.Fail = $"({-extra} missing)";
                }
                else if (count % alternative_casts_for_group != 0)
                    row.Fail = $"(not a multiple of {alternative_casts_for_group})";
                Rows.Add(row);
                if (cast_group.AlternateCasts && count > 0)
                {
                    var count_per_ac = count / alternative_casts_for_group;
                    var ac_assigned = new Dictionary<AlternativeCast, int>();
                    int not_assigned = 0;
                    foreach (var group in cast_group.Members.GroupBy(a => a.AlternativeCast))
                    {
                        if (group.Key is AlternativeCast alternative_cast)
                            ac_assigned.Add(alternative_cast, group.Count());
                        else
                            not_assigned = cast_group.Members.Where(a => a.AlternativeCast == null).Count();
                    }
                    foreach (var alternative_cast in alternative_casts)
                    {
                        if (!ac_assigned.TryGetValue(alternative_cast, out int assigned))
                            assigned = ac_assigned[alternative_cast] = 0;
                        var ac_row = new Row { Success = $"{assigned.Plural(cast_group.Abbreviation)} assigned to {alternative_cast.Name}" };
                        var ac_extra = assigned - count_per_ac;
                        if (ac_extra > 0)
                            ac_row.Fail = $"({ac_extra} too many)";
                        else if (ac_extra < 0)
                            ac_row.Fail = $"({-ac_extra} missing)";
                        Rows.Add(ac_row);
                    }
                    if (!ac_assigned.Values.AllEqual())
                        Rows.Add(new Row { Fail = $"Alternative casts of {cast_group.Abbreviation}s are not equal" });
                    if (not_assigned > 0)
                        Rows.Add(new Row { Fail = $"{not_assigned.Plural(cast_group.Abbreviation)} not assigned to an alternative cast" });
                }
                else
                {
                    var wrongly_assigned = cast_group.Members.Where(a => a.AlternativeCast != null).Count();
                    if (wrongly_assigned > 0)
                        Rows.Add(new Row { Fail = $"{wrongly_assigned.Plural(cast_group.Abbreviation)} wrongly assigned to an alternative cast" });
                }
                sum += count;
            }
            Rows.Insert(0, new Row { Success = $"{sum} Cast Selected" });
            // same cast sets
            if (cast_groups.Any(cg => cg.AlternateCasts))
            {
                var same_cast_sets = await c.SameCastSets.ToArrayAsync();
                if (same_cast_sets.Length > 0)
                {
                    var total_applicants = same_cast_sets.Sum(set => set.Applicants.Count);
                    var row = new Row { Success = $"{same_cast_sets.Length} same-cast sets including {total_applicants} applicants" };
                    var incomplete_sets = same_cast_sets.Count(set => set.Applicants.Count < 2);
                    if (incomplete_sets > 0)
                        row.Fail = $"({incomplete_sets} incomplete)";
                    Rows.Add(row);
                    foreach (var failed_set in same_cast_sets.Where(set => !set.VerifyAlternativeCasts(out _)))
                        Rows.Add(new Row { Fail = $"{failed_set.Description} are not all in the same cast" });
                }
            }
            // tags
            var tags = await c.Tags.Include(cg => cg.Members).ToArrayAsync();
            foreach (var tag in tags)
            {
                Rows.Add(new Row { Success = $"{tag.Members.Count} tagged {tag.Name}" });
                foreach (var group in tag.Members.GroupBy(a => (a.CastGroup, a.AlternativeCast)))
                {
                    var count = group.Count();
                    var alternative_cast = group.Key.AlternativeCast == null ? "" : $" in the {group.Key.AlternativeCast.Name} cast";
                    if (group.Key.CastGroup == null)
                        Rows.Add(new Row { Fail = $"{count} rejected applicants tagged {tag.Name}{alternative_cast}" });
                    else if (tag.CountFor(group.Key.CastGroup) is uint required_count)
                    {
                        if ((group.Key.AlternativeCast == null) == group.Key.CastGroup.AlternateCasts)
                            required_count = 0;
                        if (required_count != count)
                            Rows.Add(new Row { Fail = $"{count} {group.Key.CastGroup.Abbreviation}s tagged {tag.Name}{alternative_cast} (required {required_count})" });
                    }
                }
            }
            // cast number range and missing numbers
            int min_cast_num = 1;
            int cast_num = min_cast_num;
            var missing_nums = new List<int>();
            var incomplete_nums = new List<int>();
            foreach (var group in cast_groups.SelectMany(cg => cg.Members).OrderBy(a => a.CastNumber).GroupBy(a => a.CastNumber))
            {
                if (group.Key == null)
                {
                    Rows.Add(new Row { Fail = $"{group.Count()} cast members without a cast number" });
                    continue;
                }
                while (cast_num != group.Key)
                    missing_nums.Add(cast_num++);
                var cast = group.ToArray();
                var expected_length = cast[0].CastGroup!.AlternateCasts ? alternative_casts.Length : 1;
                if (cast.Length != expected_length)
                    incomplete_nums.Add(cast_num);
                cast_num++;
            }
            int max_cast_num = cast_num - 1;
            if (max_cast_num >= min_cast_num)
                Rows.Add(new Row
                {
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
