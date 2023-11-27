using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Desktop.ViewModels
{
    public interface ISelectableApplicant
    {
        Applicant Applicant { get; }
        Criteria[] PrimaryCriterias { get; }
        bool IsSelected { get; set; }
        string FirstName { get; }
        string LastName { get; }
        string SelectionText { get; }
        IEnumerable<string> ExistingRoles { get; }
        IEnumerable<string> UnavailabilityReasons { get; }
        IEnumerable<string> IneligibilityReasons { get; }
    }
}
