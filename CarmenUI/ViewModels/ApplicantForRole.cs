using CastingEngine;
using ShowModel.Applicants;
using ShowModel.Criterias;
using ShowModel.Structure;
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

        public string CastNumberAndCast => $"{Applicant.CastNumber}{Applicant.AlternativeCast?.Initial}";

        /// <summary>Indicies match Criterias</summary>
        public uint[] Marks { get; init; }

        /// <summary>Not including this role, even if already cast in it</summary>
        public double[] ExistingRoles { get; init; }

        public int OverallAbility => Applicant.OverallAbility;

        public CastGroupAndCast CastGroupAndCast { get; init; }

        public Availability Availability { get; init; }
        
        public bool IsAvailable => Availability == Availability.Available;

        public IEnumerable<string> UnavailabilityReasons
        {
            get
            {
                var av = Availability;
                if (av.HasFlag(Availability.AlreadyInItem))
                    yield return "Already cast in ITEM";
                if (av.HasFlag(Availability.AlreadyInNonMultiSection))
                    yield return "Already cast in SECTION";
                if (av.HasFlag(Availability.InPreviousItem))
                    yield return "Cast in NEXT item";
                if (av.HasFlag(Availability.InNextItem))
                    yield return "Cast in PREVIOUS item";
            }
        }

        public string CommaSeparatedUnavailabilityReason => string.Join(", ", UnavailabilityReasons);

        public ApplicantForRole(ICastingEngine engine, Applicant applicant, Role role, Criteria[] criterias)
        {
            this.Applicant = applicant;
            CastGroupAndCast = new CastGroupAndCast(Applicant);
            this.role = role;
            Criterias = criterias;
            Suitability = engine.SuitabilityOf(applicant, role);
            Marks = new uint[Criterias.Length];
            for (var i = 0; i < Marks.Length; i++)
                Marks[i] = applicant.MarkFor(Criterias[i]);
            ExistingRoles = new double[Criterias.Length];
            for (var i = 0; i < ExistingRoles.Length; i++)
                ExistingRoles[i] = engine.CountRoles(applicant, Criterias[i], role);
            Availability = engine.AvailabilityOf(applicant, role);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
