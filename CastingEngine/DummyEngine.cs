using ShowModel.Applicants;
using ShowModel.Criterias;
using ShowModel.Requirements;
using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastingEngine
{
    /// <summary>
    /// For testing only, does not nessesarily produce valid casting, only valid at a data model/type level.
    /// </summary>
    public class DummyEngine : ICastingEngine
    {
        /// <summary>Dummy value is just the applicant's Overall ability, irrelavant to the role</summary>
        public double SuitabilityOf(Applicant applicant, Role role) => applicant.OverallAbility / 100.0;

        /// <summary>Dummy value counts top level AbilityExact/AbilityRange requirements only</summary>
        public double CountRoles(Applicant applicant, Criteria criteria, Role? excluding_role)
            => applicant.Roles.Where(r => r != excluding_role)
            .Where(r => r.Requirements.Any(req => req is ICriteriaRequirement cr && cr.Criteria == criteria))
            .Count();

        /// <summary>Dummy value is always available unless they are already cast in a role in any of the items this role is in, except this role</summary>
        public Availability AvailabilityOf(Applicant applicant, Role role)
            => role.Items.SelectMany(i => i.Roles).Distinct()
            .Where(r => r != role)
            .Any(r => r.Cast.Contains(applicant))
            ? Availability.AlreadyInItem : Availability.Available;
    }
}
