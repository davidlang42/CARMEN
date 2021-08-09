using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public struct CastGroupAndCast
    {
        public CastGroup CastGroup { get; set; }
        public AlternativeCast? Cast { get; set; }

        public string Name => Cast == null ? CastGroup.Name : $"{CastGroup.Name} ({Cast.Name})";
        public string Abbreviation => Cast == null ? CastGroup.Abbreviation : $"{CastGroup.Abbreviation}-{Cast.Initial}";

        public CastGroupAndCast(CastGroup cast_group, AlternativeCast? alternative_cast = null)
        {
            CastGroup = cast_group;
            Cast = alternative_cast;
        }

        public CastGroupAndCast(Applicant applicant)
            : this(applicant.CastGroup ?? throw new ApplicationException("Applicant does not have a CastGroup."),
                  applicant.AlternativeCast)
        { }

        public static IEnumerable<CastGroupAndCast> Enumerate(IEnumerable<CastGroup> cast_groups, IEnumerable<AlternativeCast> alternative_casts)
        {
            foreach (var cast_group in cast_groups)
            {
                if (cast_group.AlternateCasts)
                    foreach (var alternative_cast in alternative_casts)
                        yield return new CastGroupAndCast(cast_group, alternative_cast);
                else
                    yield return new CastGroupAndCast(cast_group);
            }
        }
    }
}
