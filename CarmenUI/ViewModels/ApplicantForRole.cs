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
        private Criteria[] criterias;

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

        public double Suitability => new Random().NextDouble(); //TODO (2) populate suitability
        public string FirstName => Applicant.FirstName;
        public string LastName => Applicant.LastName;

        public string CastNumberAndCast => $"{Applicant.CastNumber}{Applicant.AlternativeCast?.Initial}";

        public uint[] Marks => new uint[] { 10, 20, 30 };//TODO (2) populate marks

        /// <summary>Not including this role, even if already cast in it</summary>
        public double[] ExistingRoles => new double[] { 1, 2, 3 };//TODO (2) populate existing roles

        public int OverallAbility => Applicant.OverallAbility;

        public CastGroupAndCast CastGroupAndCast { get; init; }

        public Availability Availability { get; }//TODO (2) populate availability
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

        public ApplicantForRole(Applicant applicant, Role role, Criteria[] criterias)
        {
            this.Applicant = applicant;
            CastGroupAndCast = new CastGroupAndCast(Applicant);
            this.role = role;
            this.criterias = criterias;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    [Flags]
    public enum Availability
    {
        Available = 0,
        AlreadyInItem = 1,
        AlreadyInNonMultiSection = 2,
        InPreviousItem = 4,
        InNextItem = 8
    }
}
