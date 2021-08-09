using CarmenUI.Converters;
using Microsoft.EntityFrameworkCore;
using ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class AuditionSummary : Summary
    {
        public override async Task LoadAsync(ShowContext c)
        {
            StartLoad();
            await c.Applicants.LoadAsync();
            await c.Criterias.LoadAsync();
            var auditioned = await c.Applicants.Local.Where(a => a.HasAuditioned(c.Criterias.Local)).ToListAsync();
            Rows.Add(new Row { Success = $"{auditioned.Count} Applicants Auditioned" });
            await c.CastGroups.Include(cg => cg.Requirements).LoadAsync();
            foreach (var cast_group in c.CastGroups.Local)
            {
                var applicant_count = await auditioned.CountAsync(a => cast_group.Requirements.All(r => r.IsSatisfiedBy(a)));//LATER paralleise
                var row = new Row { Success = $"{applicant_count} eligible for {cast_group.Name}" };
                if (applicant_count < cast_group.RequiredCount)
                    row.Fail = $"({cast_group.RequiredCount} required)";
                Rows.Add(row);
            }
            var not_auditioned = c.Applicants.Local.Count - auditioned.Count;
            if (not_auditioned > 0)
                Rows.Add(new Row { Fail = $"{not_auditioned.Plural("Appliant has","Applicants have")} not auditioned" });
            FinishLoad(true);
        }
    }
}
