using Carmen.CastingEngine;
using Carmen.CastingEngine.Allocation;
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

namespace Carmen.Desktop.ViewModels
{
    public class ApplicantForRole : ISelectableApplicant, INotifyPropertyChanged
    {
        public Applicant Applicant { get; init; }
        public Criteria[] PrimaryCriterias { get; init; }
        public Role Role { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsCastGroupNeeded => Role.CountFor(Applicant.CastGroup!) > 0;

        public string CastGroupNotNeededReason => IsCastGroupNeeded ? "" : $"{Applicant.CastGroup!.Abbreviation} are not needed for {Role.Name}";

        public bool IsSelected
        {
            get => Role.Cast.Contains(Applicant);
            set
            {
                if (value)
                {
                    if (!Role.Cast.Contains(Applicant))
                        Role.Cast.Add(Applicant);
                }
                else
                {
                    if (Role.Cast.Contains(Applicant))
                        Role.Cast.Remove(Applicant);
                }
                OnPropertyChanged();
            }
        }

        public double Suitability { get; init; }
        public string FirstName => Applicant.FirstName;
        public string LastName => Applicant.LastName;

        public string RoleName => Role.Name;

        public string? CastNumberAndCast => Applicant.CastNumberAndCast;

        /// <summary>Indicies match PrimaryCriterias</summary>
        public string[] Marks { get; init; }

        /// <summary>Indicies match PrimaryCriterias</summary>
        public double[] ExistingRoleCounts { get; init; }

        /// <summary>Not including this role, even if already cast in it.</summary>
        public IEnumerable<string> ExistingRoles
            => Applicant.Roles.Where(r => r != Role)
            .Select(r => $"'{r.Name}' in {string.Join(", ", r.Items.Select(i => i.Name))}");

        public string OverallAbility { get; init; }

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

        public string? SelectionText => $"Allocate {RoleName} to {FirstName}";

        public ApplicantForRole(IAllocationEngine engine, Applicant applicant, Role role, Criteria[] primary_criterias)
        {
            this.Applicant = applicant;
            CastGroupAndCast = new CastGroupAndCast(Applicant);
            this.Role = role;
            PrimaryCriterias = primary_criterias;
            Suitability = engine.SuitabilityOf(applicant, role);
            OverallAbility = engine.OverallAbility(applicant).ToString();
            if (engine.MaxOverallAbility == 100)
                OverallAbility += "%";
            Marks = new string[PrimaryCriterias.Length];
            for (var i = 0; i < Marks.Length; i++)
                Marks[i] = applicant.FormattedMarkFor(PrimaryCriterias[i]);
            ExistingRoleCounts = new double[PrimaryCriterias.Length];
            for (var i = 0; i < ExistingRoleCounts.Length; i++)
                ExistingRoleCounts[i] = engine.CountRoles(applicant, PrimaryCriterias[i], role);
            Availability = engine.AvailabilityOf(applicant, role);
            Eligibility = engine.EligibilityOf(applicant, role);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
