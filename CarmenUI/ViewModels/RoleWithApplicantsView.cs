﻿using ShowModel.Applicants;
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

namespace CarmenUI.ViewModels
{
    public class RoleWithApplicantsView : RoleView
    {
        public CastGroupAndCast[] CastGroupsByCast { get; init; }

        /// <summary>Indicies match CastGroupsByCast</summary>
        public uint[] RequiredCast { get; init; }

        /// <summary>Indicies match CastGroupsByCast</summary>
        public uint[] SelectedCast //LATER check if re-calculating this each time is too slow, maybe cache and update on each applicant change
        {
            get
            {
                var dictionary = Applicants.Where(a => a.IsSelected).GroupBy(a => a.CastGroupAndCast).ToDictionary(g => g.Key, g => (uint)g.Count());
                var result = new uint[CastGroupsByCast.Length];
                for (var i = 0; i < CastGroupsByCast.Length; i++)
                    if (dictionary.TryGetValue(CastGroupsByCast[i], out var count))
                        result[i] = count;
                return result;
            }
        }

        public ApplicantForRole[] Applicants { get; init; }

        /// <summary>Array arguments are not expected not to change over the lifetime of this View.
        /// Elements of the array may be monitored for changes, but the collection itself is not.</summary>
        public RoleWithApplicantsView(Role role, CastGroupAndCast[] cast_groups_by_cast, Criteria[] criterias, Applicant[] applicants)
            : base(role)
        {
            CastGroupsByCast = cast_groups_by_cast;
            RequiredCast = new uint[CastGroupsByCast.Length];
            for (var i = 0; i < CastGroupsByCast.Length; i++)
                RequiredCast[i] = role.CountFor(CastGroupsByCast[i].CastGroup);
            Applicants = applicants.Select(a =>
            {
                var av = new ApplicantForRole(a, role, criterias);
                av.PropertyChanged += ApplicantForRole_PropertyChanged;
                return av;
            }).ToArray();
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