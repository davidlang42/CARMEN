﻿using CastingEngine;
using ShowModel.Applicants;
using ShowModel.Criterias;
using ShowModel.Structure;
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
        public EditableRoleWithApplicantsView(ICastingEngine engine, Role role, CastGroupAndCast[] cast_groups_by_cast, Criteria[] criterias, Applicant[] applicants)
            : base(role, cast_groups_by_cast)
        {
            var required_cast_groups = role.CountByGroups.Where(cbg => cbg.Count != 0).Select(cbg => cbg.CastGroup).ToHashSet();
            Applicants = applicants.Where(a => required_cast_groups.Contains(a.CastGroup!)).Select(a =>
            {
                var av = new ApplicantForRole(engine, a, role, criterias);
                av.PropertyChanged += ApplicantForRole_PropertyChanged;
                return av;
            }).ToArray();
            var view = (CollectionView)CollectionViewSource.GetDefaultView(Applicants);
            view.GroupDescriptions.Add(new PropertyGroupDescription($"{nameof(ApplicantForRole.CastGroupAndCast)}.{nameof(CastGroupAndCast.Name)}"));
            ConfigureFiltering(false);
            ConfigureSorting();
        }

        public void ConfigureFiltering(bool show_unavailable)
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(Applicants);
            view.Filter = show_unavailable ? null : av => ((ApplicantForRole)av).Availability.IsAvailable;
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