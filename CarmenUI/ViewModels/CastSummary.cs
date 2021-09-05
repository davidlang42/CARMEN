﻿using Microsoft.EntityFrameworkCore;
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
            //TODO add same cast sets (X same cast sets including Y applicants, error if any empty or single applicant) also verify that they are respected
            StartLoad();
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
            var tags = await c.Tags.Include(cg => cg.Members).ToArrayAsync();
            foreach (var tag in tags)
            {
                var row = new Row { Success = $"{tag.Members.Count} tagged {tag.Name}" };
                if (tag.Members.GroupBy(a => a.CastGroup).Any(g => g.Key == null || (tag.CountFor(g.Key) is uint required_count && required_count != g.Count()))) //LATER does this need await?
                    row.Fail = $"(doesn't match required)";
                Rows.Add(row);
            }
            FinishLoad(cancel, sum == 0);
        }
    }
}
