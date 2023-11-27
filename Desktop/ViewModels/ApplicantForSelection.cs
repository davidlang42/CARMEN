using Carmen.CastingEngine.Allocation;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Desktop.ViewModels
{
    public abstract class ApplicantForSelection : ISelectableApplicant, INotifyPropertyChanged
    {
        public Applicant Applicant { get; init; }
        public Criteria[] PrimaryCriterias { get; init; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public abstract bool IsSelected { get; set; }

        public string FirstName => Applicant.FirstName;
        public string LastName => Applicant.LastName;

        public IEnumerable<string> ExistingRoles => Enumerable.Empty<string>();
        public IEnumerable<string> UnavailabilityReasons => Enumerable.Empty<string>(); //TODO anything to populate here?
        public IEnumerable<string> IneligibilityReasons => Enumerable.Empty<string>(); //TODO anything to populate here?

        public abstract string SelectionText { get; }

        public ApplicantForSelection(Applicant applicant, Criteria[] criterias)
        {
            Applicant = applicant;
            PrimaryCriterias = criterias; // all criterias are useful at the selection phase, not just primary
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
