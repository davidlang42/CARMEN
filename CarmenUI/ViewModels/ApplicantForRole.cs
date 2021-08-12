﻿using Carmen.CastingEngine;
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
using System.Windows;

namespace CarmenUI.ViewModels
{
    public class ApplicantForRole : INotifyPropertyChanged
    {
        public Applicant Applicant;//LATER should really be private
        private Role role;
        public Criteria[] Criterias { get; init; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsSelected
        {
            get => role.Cast.Contains(Applicant);
            set
            {
                if (value)
                {
                    if (!role.Cast.Contains(Applicant))
                        role.Cast.Add(Applicant);
                }
                else
                {
                    if (role.Cast.Contains(Applicant))
                        role.Cast.Remove(Applicant);
                }
                OnPropertyChanged();
            }
        }

        public double Suitability { get; init; }
        public string FirstName => Applicant.FirstName;
        public string LastName => Applicant.LastName;

        public string? CastNumberAndCast => Applicant.CastNumberAndCast;

        /// <summary>Indicies match Criterias</summary>
        public uint[] Marks { get; init; }

        /// <summary>Not including this role, even if already cast in it</summary>
        public double[] ExistingRoles { get; init; }

        public int OverallAbility { get; init; }

        public CastGroupAndCast CastGroupAndCast { get; init; }

        public Availability Availability { get; init; }

        public Eligibility Eligibility { get; init; }

        public IEnumerable<string> UnavailabilityReasons
        {
            get
            {
                var av = Availability;
                if (av.IsAlreadyInItem)
                    foreach (var item in av.AlreadyInItems!)
                        yield return $"Already cast in {item.Name}";
                if (av.IsAlreadyInNonMultiSection)
                    foreach (var nms in av.AlreadyInNonMultiSections!)
                        yield return $"Already cast in {nms.NonMultiSection.Name}";
                if (av.IsInAdjacentItem)
                    foreach (var adj in av.InAdjacentItems!)
                    {
                        var msg = $"Cast in {adj.Adjacency.ToString().ToLower()} item ({adj.AlreadyInItem.Name})";
                        if (adj.NonConsecutiveSection is not ShowRoot)
                            msg += $" within {adj.NonConsecutiveSection.Name}";
                        yield return msg;
                    }
            }
        }

        public IEnumerable<string> IneligibilityReasons => Eligibility.RequirementsNotMet.Select(r => r.Reason ?? $"'{r.Name}' requirement not met");

        public string CommaSeparatedUnavailabilityReason => string.Join(", ", UnavailabilityReasons);

        public string CommaSeparatedIneligibilityReason => string.Join(", ", IneligibilityReasons);

        public ApplicantForRole(ICastingEngine engine, Applicant applicant, Role role, Criteria[] criterias)
        {
            this.Applicant = applicant;
            CastGroupAndCast = new CastGroupAndCast(Applicant);
            this.role = role;
            Criterias = criterias;
            Suitability = engine.SuitabilityOf(applicant, role);
            OverallAbility = engine.OverallAbility(applicant);
            Marks = new uint[Criterias.Length];
            for (var i = 0; i < Marks.Length; i++)
                Marks[i] = applicant.MarkFor(Criterias[i]);
            ExistingRoles = new double[Criterias.Length];
            for (var i = 0; i < ExistingRoles.Length; i++)
                ExistingRoles[i] = engine.CountRoles(applicant, Criterias[i], role);
            Availability = engine.AvailabilityOf(applicant, role);
            Eligibility = engine.EligibilityOf(applicant, role);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
