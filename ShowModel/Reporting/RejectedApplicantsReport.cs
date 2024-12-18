﻿using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Reporting
{
    public class RejectedApplicantsReport : ApplicantsReport
    {
        public new const string DefaultReportType = "Rejected Applicants";

        public override string ReportType => DefaultReportType;

        public RejectedApplicantsReport(Criteria[] criterias, Tag[] tags)
            : base(criterias, tags)
        { }

        public override Task SetData(IEnumerable<Applicant> data) => base.SetData(data.Where(a => a.CastGroup == null));
    }
}
