﻿using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Reporting
{
    public class AcceptedApplicantsReport : ApplicantsReport
    {
        public new const string DefaultReportType = "Accepted Applicants";

        public override string ReportType => DefaultReportType;

        public AcceptedApplicantsReport(Criteria[] criterias, Tag[] tags)
            : base(criterias, tags)
        { }

        public override Task SetData(IEnumerable<Applicant> data) => base.SetData(data.Where(a => a.CastGroup != null));
    }
}
