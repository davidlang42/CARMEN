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
    /// Interface for CastingEngine functions relating to Role allocation
    /// </summary>
    public interface IAllocationEngine
    {
        /// <summary>Determine the recommended order in which the roles should be cast.
        /// This should return all roles found within the specified set of items, regardless of whether
        /// or not they are already cast.</summary>
        IEnumerable<Role> IdealCastingOrder(IEnumerable<Item> items_in_order);

        /// <summary>Finds the next Role in IdealCastingOrder() which has not yet been fully cast,
        /// optionally exlcuding a specified role.</summary>
        Role? NextUncastRole(IEnumerable<Item> items_in_order, AlternativeCast[] alternative_casts, Role? excluding_role = null)
        {
            var e = IdealCastingOrder(items_in_order)
                .Where(r => r.CastingStatus(alternative_casts) != Role.RoleStatus.FullyCast)
                .GetEnumerator();
            if (!e.MoveNext())
                return null;
            if (e.Current == excluding_role)
                if (!e.MoveNext())
                    return null;
            return e.Current;
        }

        /// <summary>Calculate the suitability of an applicant for a role, regardless of availability and eligibility.
        /// Value returned will be between 0 and 1 (inclusive).</summary>
        double SuitabilityOf(Applicant applicant, Role role);

        /// <summary>Determine if an applicant is eligible to be cast in a role
        /// (ie. whether all minimum requirements of the role are met)</summary>
        Eligibility EligibilityOf(Applicant applicant, Role role)
        {
            var requirements_not_met = new HashSet<Requirement>();
            foreach (var req in role.Requirements)
                _ = req.IsSatisfiedBy(applicant, requirements_not_met);
            return new Eligibility
            {
                RequirementsNotMet = requirements_not_met.ToArray()
            };
        }

        /// <summary>Count the number of roles an applicant has which require a certain criteria,
        /// optionally excluding a specified role. Depending on implementation, this may return a
        /// whole number of actual roles, or a sum of prorated roles based on the importance of
        /// the specified criteria to each role.</summary>
        double CountRoles(Applicant applicant, Criteria criteria, Role? excluding_role);

        /// <summary>Determine if an applicant is available to be cast in a role
        /// (eg. already cast in the same item, an adjacent item, or within a section where AllowMultipleRoles==FALSE)</summary>
        Availability AvailabilityOf(Applicant applicant, Role role);

        /// <summary>Pick the cast for a role</summary>
        IEnumerable<Applicant> PickCast(IEnumerable<Applicant> applicants, Role role, IEnumerable<AlternativeCast> alternative_casts);
    }
}
