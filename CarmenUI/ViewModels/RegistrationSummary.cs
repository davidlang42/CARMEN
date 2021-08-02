using CarmenUI.Converters;
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
    public class RegistrationSummary : Summary
    {
        public async Task LoadAsync(IReadOnlyCollection<Applicant> applicants, IReadOnlyCollection<CastGroup> cast_groups,
            IReadOnlyCollection<Criteria> criterias)
        {
            StartLoad();
            Rows.Add(new Row { Success = $"{applicants.Count} Applicants Registered" });
            foreach (var cast_group in cast_groups)
            {
                var applicant_count = await applicants.CountAsync(a => cast_group.Requirements.All(r => r.IsSatisfiedBy(a)));//LATER paralleise
                var row = new Row { Success = $"{applicant_count} eligible for {cast_group.Name}" };
                if (applicant_count < cast_group.RequiredCount)
                    row.Fail = $"({cast_group.RequiredCount} required)";
                Rows.Add(row);
            }
            //TODO don't use criterias here
            var incomplete = await applicants.CountAsync(a => IsApplicantComplete.Check(a, criterias));//LATER paralleise
            if (incomplete > 0)
                Rows.Add(new Row { Fail = $"{incomplete} Applicants are Incomplete" });
            FinishLoad(true);
        }
    }
}
