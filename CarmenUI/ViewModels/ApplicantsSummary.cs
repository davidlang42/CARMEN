using CarmenUI.Converters;
using ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class ApplicantsSummary : Summary
    {
        public override async Task LoadAsync(ShowContext context)
        {
            StartLoad();
            var applicants = (await context.ColdLoadAsync(c => c.Applicants)).ToList();
            Rows.Add(new Row { Success = $"{applicants.Count} Applicants Registered" });
            var cast_groups = (await context.ColdLoadAsync(c => c.CastGroups)).ToList();
            foreach (var cast_group in cast_groups)
            {
                var applicant_count = applicants.Count(a => cast_group.Requirements.All(r => r.IsSatisfiedBy(a)));//TODO hangs, doesnt return properly
                var row = new Row { Success = $"{applicant_count} eligible for {cast_group.Name}" };
                if (applicant_count < cast_group.RequiredCount)
                    row.Fail = $"({cast_group.RequiredCount} required)";
                Rows.Add(row);
            }
            var all_criterias = (await context.ColdLoadAsync(c => c.Criterias)).ToList();
            var incomplete = applicants.Count(a => IsApplicantComplete.Check(a, all_criterias));//TODO hangs, doesnt return properly
            if (incomplete > 0)
                Rows.Add(new Row { Fail = $"{incomplete} Applicants are Incomplete" });
            FinishLoad(true);
        }
    }
}
