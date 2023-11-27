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
    public class ApplicantForSelection : ISelectableApplicant, INotifyPropertyChanged
    {
        public Applicant Applicant { get; init; }
        public Criteria[] PrimaryCriterias { get; init; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsSelected
        {
            get => true; // so that colour images are shown
            set => throw new InvalidOperationException();
        }

        public string FirstName => Applicant.FirstName;
        public string LastName => Applicant.LastName;

        public IEnumerable<string> ExistingRoles => Enumerable.Empty<string>();
        public IEnumerable<string> UnavailabilityReasons => Enumerable.Empty<string>();
        public IEnumerable<string> IneligibilityReasons => Enumerable.Empty<string>();

        public string? SelectionText => null; // this hides the checkbox completely

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
