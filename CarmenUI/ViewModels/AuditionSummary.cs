﻿using ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class AuditionSummary : ApplicantsSummary
    {
        public override async Task LoadAsync(ShowContext context)
        {
            StartLoad();
            await base.LoadAsync(context);
            var applicants = (await context.ColdLoadAsync(c => c.Applicants)).ToList();
            var all_criterias = (await context.ColdLoadAsync(c => c.Criterias)).ToList();
            var not_auditioned = await applicants.CountAsync(a => !a.HasAuditioned(all_criterias));//LATER paralleise
            if (not_auditioned > 0)
                Rows.Add(new Row { Fail = $"{not_auditioned.Plural("Appliant has","Applicants have")} not auditioned" });
            FinishLoad(true);
        }
    }
}
