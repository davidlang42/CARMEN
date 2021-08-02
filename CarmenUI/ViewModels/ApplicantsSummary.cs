﻿using CarmenUI.Converters;
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
            var concrete_class = GetType() == typeof(ApplicantsSummary); // don't update status if this is a subclass
            if (concrete_class)
                StartLoad();
            var applicants = (await context.ColdLoadAsync(c => c.Applicants)).ToList();
            Rows.Add(new Row { Success = $"{applicants.Count} Applicants Registered" });
            var cast_groups = (await context.ColdLoadAsync(c => c.CastGroups)).ToList();
            foreach (var cast_group in cast_groups)
            {
                var applicant_count = await applicants.CountAsync(a => cast_group.Requirements.All(r => r.IsSatisfiedBy(a)));//LATER paralleise
                var row = new Row { Success = $"{applicant_count} eligible for {cast_group.Name}" };
                if (applicant_count < cast_group.RequiredCount)
                    row.Fail = $"({cast_group.RequiredCount} required)";
                Rows.Add(row);
            }
            if (concrete_class)
                FinishLoad(true);
        }
    }
}
