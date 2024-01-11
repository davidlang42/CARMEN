﻿using Carmen.ShowModel.Applicants;
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

        //TODO remove if unused
        //public IEnumerable<ParallelRole> SelectedForRoles => applicantForRoles.Where(kvp => kvp.Value.IsSelected).Select(kvp => kvp.Key);

        public ParallelApplicant(ParallelCastingView casting_view, Applicant applicant, ApplicantForRole[] applicant_for_roles, Criteria[] primary_criterias)
        {
            PrimaryCriterias = primary_criterias;
            castingView = casting_view;
            Applicant = applicant;
            ApplicantForRoles = applicant_for_roles;
            foreach (var afr in ApplicantForRoles)
            {
                afr.PropertyChanged += ApplicantForRole_PropertyChanged; ;
            }
            castingView.PropertyChanged += CastingView_PropertyChanged;
            //TODO remove if unused
            //Action? double_click = null, IEnumerable<(string, Action)>? right_click = null
            //DoubleClick = double_click;
            //if (right_click != null)
            //{
            //    ContextMenu = new ContextMenu();
            //    foreach (var (text, action) in right_click)
            //    {
            //        var menu_item = new MenuItem() { Header = text };
            //        menu_item.Click += (s, e) => action();
            //        ContextMenu.Items.Add(menu_item);
            //    }
            //}
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

        //TODO remove if unused
        //private void ApplicantForRole_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(ApplicantForRole.IsSelected))
        //    {
        //        OnPropertyChanged(nameof(SelectedForRoles));
        //    }
        //}

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
