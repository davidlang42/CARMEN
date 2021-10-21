using CarmenUI.Converters;
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
    public class RegistrationSummary : Summary
    {
        public override async Task LoadAsync(ShowContext c, CancellationToken cancel)
        {
            StartLoad();
            var applicants = await c.Applicants.ToArrayAsync();
            Rows.Add(new Row { Success = $"{applicants.Length.Plural("Applicant")} Registered" });
            var cast_groups = await c.CastGroups.Include(cg => cg.Requirements).ToArrayAsync();
            foreach (var cast_group in cast_groups)
            {
                var applicant_count = await applicants.CountAsync(a => cast_group.Requirements.All(r => r.IsSatisfiedBy(a)));
                var row = new Row { Success = $"{applicant_count} eligible for {cast_group.Name}" };
                if (applicant_count < cast_group.RequiredCount)
                    row.Fail = $"({cast_group.RequiredCount} required)";
                Rows.Add(row);
            }
            await c.Criterias.LoadAsync();
            var incomplete = await applicants.CountAsync(a => !a.IsRegistered);
            if (incomplete > 0)
                Rows.Add(new Row { Fail = $"{incomplete.Plural("Applicant is", "Applicants are")} Incomplete" });
            FinishLoad(cancel, applicants.Length == 0 || incomplete > 0);
        }
    }
}
