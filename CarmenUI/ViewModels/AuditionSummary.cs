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
        public override async Task LoadAsync(ShowContext context)
        {
            StartLoad();
            var applicants = (await context.ColdLoadAsync(c => c.Applicants)).ToList();
            var all_criterias = (await context.ColdLoadAsync(c => c.Criterias)).ToList();
            var auditioned = applicants.Where(a => a.HasAuditioned(all_criterias)).ToList();
            Rows.Add(new Row { Success = $"{auditioned.Count} Applicants Auditioned" });
            var cast_groups = (await context.ColdLoadAsync(c => c.CastGroups)).ToList();
            foreach (var cast_group in cast_groups)
            {
                var applicant_count = await auditioned.CountAsync(a => cast_group.Requirements.All(r => r.IsSatisfiedBy(a)));//LATER paralleise
                var row = new Row { Success = $"{applicant_count} eligible for {cast_group.Name}" };
                if (applicant_count < cast_group.RequiredCount)
                    row.Fail = $"({cast_group.RequiredCount} required)";
                Rows.Add(row);
            }
            var not_auditioned = applicants.Count - auditioned.Count;
            if (not_auditioned > 0)
                Rows.Add(new Row { Fail = $"{not_auditioned.Plural("Appliant has","Applicants have")} not auditioned" });
            FinishLoad(true);
        }
    }
}
