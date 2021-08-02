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
    public class RegistrationSummary : Summary
    {
        public override async Task LoadAsync(ShowContext c)
        {
            StartLoad();
            await c.Applicants.LoadAsync();
            Rows.Add(new Row { Success = $"{c.Applicants.Local.Count} Applicants Registered" });
            await c.CastGroups.Include(cg => cg.Requirements).LoadAsync();
            foreach (var cast_group in c.CastGroups.Local)
            {
                var applicant_count = await c.Applicants.Local.CountAsync(a => cast_group.Requirements.All(r => r.IsSatisfiedBy(a)));//LATER paralleise
                var row = new Row { Success = $"{applicant_count} eligible for {cast_group.Name}" };
                if (applicant_count < cast_group.RequiredCount)
                    row.Fail = $"({cast_group.RequiredCount} required)";
                Rows.Add(row);
            }
            await c.Criterias.LoadAsync();
            var incomplete = await c.Applicants.Local.CountAsync(a => !a.IsRegistered);//LATER paralleise
            if (incomplete > 0)
                Rows.Add(new Row { Fail = $"{incomplete} Applicants are Incomplete" });
            FinishLoad(true);
        }
    }
}
