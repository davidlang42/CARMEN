using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine
{
    /// <summary>
    /// Interface for CastingEngine functions relating to Role allocation
    /// </summary>
    public interface IAllocationEngine
    {
        /// <summary>An accessor to the IApplicantEngine used by this allocation engine</summary>
        IApplicantEngine ApplicantEngine { get; }

        /// <summary>Determine the recommended order in which the roles should be cast.
        /// This should return all roles found within the specified set of items, regardless of whether
        /// or not they are already cast.</summary>
        IEnumerable<Role> IdealCastingOrder(IEnumerable<Item> items_in_order);

        /// <summary>Finds the next Role in IdealCastingOrder() which has not yet been fully cast,
        /// optionally excluding a specified role.</summary>
        Role? NextUncastRole(IEnumerable<Item> items_in_order, AlternativeCast[] alternative_casts, Role? excluding_role = null);

        /// <summary>Calculate the suitability of an applicant for a role, regardless of availability and eligibility.
        /// Value returned will be between 0 and 1 (inclusive). This may contain logic specific to roles, and is therefore
        /// different to IApplicantEngine.SuitabilityOf(Applicant, Requirement).</summary>
        double SuitabilityOf(Applicant applicant, Role role);

        /// <summary>Count the number of roles an applicant has which require a certain criteria,
        /// optionally excluding a specified role. Depending on implementation, this may return a
        /// whole number of actual roles, or a sum of prorated roles based on the importance of
        /// the specified criteria to each role.</summary>
        double CountRoles(Applicant applicant, Criteria criteria, Role? excluding_role);

        /// <summary>Pick the best cast for a role</summary>
        IEnumerable<Applicant> PickCast(IEnumerable<Applicant> applicants, Role role, IEnumerable<AlternativeCast> alternative_casts);

        /// <summary>Pick the best cast for one or more roles, balancing talent between them</summary>
        IEnumerable<KeyValuePair<Role, IEnumerable<Applicant>>> BalanceCast(IEnumerable<Applicant> applicants, IEnumerable<Role> roles, IEnumerable<AlternativeCast> alternative_casts); //TODO is there a better return argument than this?

        /// <summary>Determine if an applicant is eligible to be cast in a role
        /// (ie. whether all minimum requirements of the role are met)</summary>
        Eligibility EligibilityOf(Applicant applicant, Role role);

        /// <summary>Determine if an applicant is available to be cast in a role
        /// (eg. already cast in the same item, an adjacent item, or within a section where AllowMultipleRoles==FALSE)</summary>
        Availability AvailabilityOf(Applicant applicant, Role role);
    }
}
