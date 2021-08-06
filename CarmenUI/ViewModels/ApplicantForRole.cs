using ShowModel.Applicants;
using ShowModel.Criterias;
using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class ApplicantForRole
    {
        private Applicant applicant;
        private Role role;
        private Criteria[] criterias;

        public double Suitability { get; }//TODO
        public string FirstName => applicant.FirstName;
        public string LastName => applicant.LastName;//TODO should I format these into name here? or with converter?

        public string CastNumberAndCast => $"{applicant.CastNumber}{applicant.AlternativeCast?.Initial}";

        public Ability[] Abilities { get; }//TODO

        public double[] ExistingRoles { get; }//TODO

        public int OverallAbility => applicant.OverallAbility;

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
