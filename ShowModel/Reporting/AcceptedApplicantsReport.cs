using Carmen.ShowModel.Applicants;
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
        public override string ReportType => "Accepted Applicants";

        public AcceptedApplicantsReport(Criteria[] criterias, Tag[] tags)
            : base(criterias, tags)
        { }

        public override void SetData(IEnumerable<Applicant> data) => base.SetData(data.Where(a => a.CastGroup != null));
    }
}
