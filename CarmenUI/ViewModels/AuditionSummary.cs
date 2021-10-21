﻿using CarmenUI.Converters;
using Microsoft.EntityFrameworkCore;
using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CarmenUI.ViewModels
{
    public class AuditionSummary : Summary
    {
        public override async Task LoadAsync(ShowContext c, CancellationToken cancel)
        {
            StartLoad();
            var applicants = await RealAsync(c.Applicants.ToArray);
            var criterias = await RealAsync(c.Criterias.ToArray);
            var auditioned = applicants.Where(a => a.HasAuditioned(criterias)).ToArray();
            Rows.Add(new Row { Success = $"{auditioned.Length} Applicants Auditioned" });
            var cast_groups = await RealAsync(c.CastGroups.Include(cg => cg.Requirements).ToArray);
            foreach (var cast_group in cast_groups)
            {
                var applicant_count = await auditioned.CountAsync(a => cast_group.Requirements.All(r => r.IsSatisfiedBy(a)));
                var row = new Row { Success = $"{applicant_count} eligible for {cast_group.Name}" };
                if (applicant_count < cast_group.RequiredCount)
                    row.Fail = $"({cast_group.RequiredCount} required)";
                Rows.Add(row);
            }
            var not_auditioned = applicants.Length - auditioned.Length;
            if (not_auditioned > 0)
                Rows.Add(new Row { Fail = $"{not_auditioned.Plural("Appliant has","Applicants have")} not auditioned" });
            FinishLoad(cancel, auditioned.Length == 0 || not_auditioned > 0);
        }
    }
}
