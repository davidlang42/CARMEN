using Carmen.ShowModel.Structure;
using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carmen.ShowModel.Applicants;
using Carmen.Mobile.Converters;

namespace Carmen.Mobile.Models
{
    internal class ApplicantDetail : IBasicListItem
    {
        public Applicant Applicant { get; init; }

        public string MainText => FullNameFormatter.Format(Applicant.FirstName, Applicant.LastName);

        public string DetailText => $"#{Applicant.CastNumberAndCast}";
    }
}
