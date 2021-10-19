using Carmen.CastingEngine.Allocation;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CarmenUI.ViewModels
{
    public class EditableRoleWithApplicantsView : RoleWithApplicantsView
    {
        public ApplicantForRole[] Applicants { get; init; }

        /// <summary>Array arguments are not expected not to change over the lifetime of this View.
        /// Elements of the array may be monitored for changes, but the collection itself is not.</summary>
        public EditableRoleWithApplicantsView(IAllocationEngine engine, Role role, CastGroupAndCast[] cast_groups_by_cast, Criteria[] primary_criterias, Applicant[] applicants, bool show_unavailable, bool show_ineligible)
            : base(role, cast_groups_by_cast)
        {
            var required_cast_groups = role.CountByGroups.Where(cbg => cbg.Count != 0).Select(cbg => cbg.CastGroup).ToHashSet();
            Applicants = applicants.Where(a => required_cast_groups.Contains(a.CastGroup!) || role.Cast.Contains(a)).Select(a =>
            {
                var av = new ApplicantForRole(engine, a, role, primary_criterias);
                av.PropertyChanged += ApplicantForRole_PropertyChanged;
                return av;
            }).ToArray();
            var view = (CollectionView)CollectionViewSource.GetDefaultView(Applicants);
            view.GroupDescriptions.Add(new PropertyGroupDescription($"{nameof(ApplicantForRole.CastGroupAndCast)}.{nameof(CastGroupAndCast.Name)}"));
            ConfigureFiltering(show_unavailable, show_ineligible);
            ConfigureSorting();
        }

        public void ConfigureFiltering(bool show_unavailable, bool show_ineligible)
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(Applicants);
            view.Filter = (show_unavailable, show_ineligible) switch
            {
                (false, false) => av => ((ApplicantForRole)av).IsSelected || (((ApplicantForRole)av).Availability.IsAvailable && ((ApplicantForRole)av).Eligibility.IsEligible),
                (false, true) => av => ((ApplicantForRole)av).IsSelected || ((ApplicantForRole)av).Availability.IsAvailable,
                (true, false) => av => ((ApplicantForRole)av).IsSelected || ((ApplicantForRole)av).Eligibility.IsEligible,
                _ => null // (true, true)
            };
        }

        public void ConfigureSorting()
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(Applicants);
            view.SortDescriptions.Add(new($"{nameof(ApplicantForRole.CastGroupAndCast)}.{nameof(CastGroupAndCast.CastGroup)}.{nameof(CastGroup.Order)}", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new($"{nameof(ApplicantForRole.CastGroupAndCast)}.{nameof(CastGroupAndCast.Cast)}.{nameof(AlternativeCast.Initial)}", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new(nameof(ApplicantForRole.Suitability), ListSortDirection.Descending));
        }

        public void ClearSelectedApplicants()
        {
            foreach (var av in Applicants)
                av.IsSelected = false;
        }

        public void SelectApplicants(IEnumerable<Applicant> applicants)
        {
            var applicant_set = applicants.ToHashSet();
            foreach (var av in Applicants)
                if (applicant_set.Contains(av.Applicant))
                    av.IsSelected = true;
        }

        private void ApplicantForRole_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(ApplicantForRole.IsSelected))
                OnPropertyChanged(nameof(SelectedCast));
        }

        protected override void DisposeInternal()
        {
            foreach (var applicant in Applicants)
                applicant.PropertyChanged -= ApplicantForRole_PropertyChanged;
        }
    }
}
