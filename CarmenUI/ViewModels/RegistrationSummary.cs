using CarmenUI.Converters;
using ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class RegistrationSummary : ApplicantsSummary
    {
        public override async Task LoadAsync(ShowContext context)
        {
            StartLoad();
            await base.LoadAsync(context);
            var applicants = (await context.ColdLoadAsync(c => c.Applicants)).ToList();
            var all_criterias = (await context.ColdLoadAsync(c => c.Criterias)).ToList();
            var incomplete = await applicants.CountAsync(a => IsApplicantComplete.Check(a, all_criterias));//LATER paralleise
            if (incomplete > 0)
                Rows.Add(new Row { Fail = $"{incomplete} Applicants are Incomplete" });
            FinishLoad(true);
        }
    }
}
