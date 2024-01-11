using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Carmen.Desktop.ViewModels
{
    public class ParallelApplicant : INotifyPropertyChanged, ISelectableApplicant
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        readonly ParallelCastingView castingView;

        public ApplicantForRole[] ApplicantForRoles { get; }

        public Applicant Applicant { get; }

        public ApplicantForRole? SelectedRole => castingView.SelectedRoleIndex == -1 ? null : ApplicantForRoles[castingView.SelectedRoleIndex];

        public ParallelApplicant(ParallelCastingView casting_view, Applicant applicant, ApplicantForRole[] applicant_for_roles, Criteria[] primary_criterias)
        {
            PrimaryCriterias = primary_criterias;
            castingView = casting_view;
            Applicant = applicant;
            ApplicantForRoles = applicant_for_roles;
            foreach (var afr in ApplicantForRoles)
            {
                afr.PropertyChanged += ApplicantForRole_PropertyChanged;
            }
            castingView.PropertyChanged += CastingView_PropertyChanged;
        }

        private void ApplicantForRole_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender != SelectedRole)
                return;
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(ApplicantForRole.IsSelected))
            {
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        private void CastingView_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(ParallelCastingView.SelectedRoleIndex))
            {
                OnPropertyChanged(nameof(SelectedRole));
                // and everything which depends on it
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(SelectionText));
                OnPropertyChanged(nameof(ExistingRoles));
                OnPropertyChanged(nameof(UnavailabilityReasons));
                OnPropertyChanged(nameof(IneligibilityReasons));
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region ISelectableApplicant
        public Criteria[] PrimaryCriterias { get; }
        public string FirstName => Applicant.FirstName;
        public string LastName => Applicant.LastName;

        public bool IsSelected
        {
            get => SelectedRole?.IsSelected ?? false;
            set
            {
                if (SelectedRole != null)
                    SelectedRole.IsSelected = value;
            }
        }

        public string? SelectionText => SelectedRole?.SelectionText; // null hides the checkbox completely

        public IEnumerable<string> ExistingRoles => SelectedRole?.ExistingRoles ?? Enumerable.Empty<string>();

        public IEnumerable<string> UnavailabilityReasons => SelectedRole?.UnavailabilityReasons ?? Enumerable.Empty<string>();

        public IEnumerable<string> IneligibilityReasons => SelectedRole?.IneligibilityReasons ?? Enumerable.Empty<string>();
        #endregion
    }
}
