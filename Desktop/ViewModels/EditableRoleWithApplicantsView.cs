﻿using Carmen.CastingEngine.Allocation;
using Carmen.CastingEngine.Audition;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
using Carmen.Desktop.Converters;
using Carmen.Desktop.Windows;
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

namespace Carmen.Desktop.ViewModels
{
    public class EditableRoleWithApplicantsView : RoleWithApplicantsView
    {
        Dictionary<ApplicantForRole, ApplicantDetailsWindow> detailsWindows = new();

        public ApplicantForRole[] Applicants { get; init; }

        readonly HashSet<CastGroup> requiredCastGroups;

        /// <summary>Array arguments are not expected not to change over the lifetime of this View.
        /// Elements of the array may be monitored for changes, but the collection itself is not.</summary>
        public EditableRoleWithApplicantsView(IAllocationEngine engine, Role role, CastGroupAndCast[] cast_groups_by_cast, Criteria[] primary_criterias, Applicant[] applicants, bool show_unavailable, bool show_ineligible, bool show_unneeded)
            : base(role, cast_groups_by_cast)
        {
            requiredCastGroups = role.CountByGroups.Where(cbg => cbg.Count != 0).Select(cbg => cbg.CastGroup).ToHashSet();
            Applicants = applicants.AsParallel().Select(a =>
            {
                var av = new ApplicantForRole(engine, a, role, primary_criterias);
                av.PropertyChanged += ApplicantForRole_PropertyChanged;
                return av;
            }).ToArray();
            var view = (CollectionView)CollectionViewSource.GetDefaultView(Applicants);
            view.GroupDescriptions.Add(new PropertyGroupDescription($"{nameof(ApplicantForRole.CastGroupAndCast)}.{nameof(CastGroupAndCast.Name)}"));
            ConfigureFiltering(show_unavailable, show_ineligible, show_unneeded);
            ConfigureSorting(new[] { (nameof(ApplicantForRole.Suitability), ListSortDirection.Descending) });
        }

        public void ConfigureFiltering(bool show_unavailable, bool show_ineligible, bool show_unneeded)
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(Applicants);
            view.Filter = (show_unavailable, show_ineligible, show_unneeded) switch
            {
                (false, false, false) => av => ((ApplicantForRole)av).IsSelected || (requiredCastGroups.Contains(((ApplicantForRole)av).Applicant.CastGroup!) && ((ApplicantForRole)av).Availability.IsAvailable && ((ApplicantForRole)av).Eligibility.IsEligible),
                (false, true, false) => av => ((ApplicantForRole)av).IsSelected || (requiredCastGroups.Contains(((ApplicantForRole)av).Applicant.CastGroup!) && ((ApplicantForRole)av).Availability.IsAvailable),
                (true, false, false) => av => ((ApplicantForRole)av).IsSelected || (requiredCastGroups.Contains(((ApplicantForRole)av).Applicant.CastGroup!) && ((ApplicantForRole)av).Eligibility.IsEligible),
                (true, true, false) => av => ((ApplicantForRole)av).IsSelected || requiredCastGroups.Contains(((ApplicantForRole)av).Applicant.CastGroup!),
                (false, false, true) => av => ((ApplicantForRole)av).IsSelected || (((ApplicantForRole)av).Availability.IsAvailable && ((ApplicantForRole)av).Eligibility.IsEligible),
                (false, true, true) => av => ((ApplicantForRole)av).IsSelected || ((ApplicantForRole)av).Availability.IsAvailable,
                (true, false, true) => av => ((ApplicantForRole)av).IsSelected || ((ApplicantForRole)av).Eligibility.IsEligible,
                (true, true, true) => null // show all
            };
        }

        public void ConfigureSorting(IEnumerable<(string, ListSortDirection)> sort_fields)
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(Applicants);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new($"{nameof(ApplicantForRole.CastGroupAndCast)}.{nameof(CastGroupAndCast.CastGroup)}.{nameof(CastGroup.Order)}", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new($"{nameof(ApplicantForRole.CastGroupAndCast)}.{nameof(CastGroupAndCast.Cast)}.{nameof(AlternativeCast.Initial)}", ListSortDirection.Ascending));
            foreach ((string sort_by, ListSortDirection direction) in sort_fields)
            {
                if (sort_by == "CastNumberAndCast")
                    view.SortDescriptions.Add(new($"{nameof(ApplicantForRole.Applicant)}.{nameof(Applicant.CastNumber)}", direction));
                else if (sort_by == "Name")
                    foreach (var sd in Properties.Settings.Default.FullNameFormat.ToSortDescriptions(direction))
                        view.SortDescriptions.Add(sd);
                else
                    view.SortDescriptions.Add(new(sort_by, direction));
            }
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
            foreach (var window in detailsWindows.Values)
                window.Close();
            detailsWindows.Clear();
            foreach (var applicant in Applicants)
                applicant.PropertyChanged -= ApplicantForRole_PropertyChanged;
            base.DisposeInternal();
        }

        public void ShowDetailsWindow(ShowConnection connection, ApplicantForRole afr, Window owner, Criteria[] criterias, IAuditionEngine audition_engine)
        {
            if (!detailsWindows.TryGetValue(afr, out var window) || window.IsClosed)
            {
                window = new ApplicantDetailsWindow(connection, criterias, audition_engine, afr)
                {
                    Owner = owner
                };
                detailsWindows[afr] = window;
                window.Show();
            }
            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;
            window.Activate();
        }
    }
}
