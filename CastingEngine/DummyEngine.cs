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

        /// <summary>Dummy value enumerates roles in item order, then by name, removing duplicates</summary>
        public IEnumerable<Role> CastingOrder(IEnumerable<Item> items_in_order)
            => items_in_order.SelectMany(i => i.Roles.OrderBy(r => r.Name)).Distinct();

        /// <summary>
        /// Dummy selection picks the number of required applicants of each type from the list of applicants, ignoring availability
        /// </summary>
        public IEnumerable<Applicant> PickCast(IEnumerable<Applicant> applicants, Role role, IEnumerable<AlternativeCast> alternative_casts)
        {
            var casts = alternative_casts.ToArray();
            foreach (var cbg in role.CountByGroups)
            {
                if (cbg.CastGroup.AlternateCasts)
                {
                    foreach(var cast in casts)
                        foreach (var applicant in applicants.Where(a => a.CastGroup == cbg.CastGroup && a.AlternativeCast == cast).Take((int)cbg.Count))
                            yield return applicant;
                }
                else
                {
                    foreach (var applicant in applicants.Where(a => a.CastGroup == cbg.CastGroup).Take((int)cbg.Count))
                        yield return applicant;
                }
            }
        }
    }
}
