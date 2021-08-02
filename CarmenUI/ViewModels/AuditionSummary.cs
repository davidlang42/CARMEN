using ShowModel;
using ShowModel.Applicants;
using ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class AuditionSummary : Summary
    {
        public async Task LoadAsync(IReadOnlyCollection<Applicant> applicants, IReadOnlyCollection<CastGroup> cast_groups,
            IReadOnlyCollection<Criteria> criterias)
        {
            StartLoad();
            var auditioned = applicants.Where(a => a.HasAuditioned(criterias)).ToList(); //LATER async/parallelise
            Rows.Add(new Row { Success = $"{auditioned.Count} Applicants Auditioned" });
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
