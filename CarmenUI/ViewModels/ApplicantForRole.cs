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
    public class ApplicantForRole : INotifyPropertyChanged //TODO
    {
        public Applicant Applicant;//LATER should really be private
        private Role role;
        private Criteria[] criterias;

        public event PropertyChangedEventHandler? PropertyChanged;

        //public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
        //    nameof(IsSelected), typeof(bool), typeof(ApplicantForRole), new PropertyMetadata(null));

        //public bool IsSelected
        //{
        //    get => (bool)GetValue(IsSelectedProperty);
        //    set => SetValue(IsSelectedProperty, value);
        //}

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
            }
        }

        public double Suitability { get; }//TODO
        public string FirstName => Applicant.FirstName;
        public string LastName => Applicant.LastName;//TODO should I format these into name here? or with converter?

        public string CastNumberAndCast => $"{Applicant.CastNumber}{Applicant.AlternativeCast?.Initial}";

        public Ability[] Abilities { get; }//TODO

        public double[] ExistingRoles { get; }//TODO

        public int OverallAbility => Applicant.OverallAbility;

        public CastGroupAndCast CastGroupAndCast => new CastGroupAndCast(Applicant);

        public Availability Availability { get; }//TODO
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
