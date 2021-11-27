using Carmen.CastingEngine.Audition;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Allocation
{
    /// <summary>
    /// Interface for CastingEngine functions relating to Role allocation
    /// </summary>
    public interface IAllocationEngine
    {
        /// <summary>An accessor to the IAuditionEngine used by this allocation engine</summary>
        IAuditionEngine AuditionEngine { get; }

        /// <summary>Determine the recommended order in which the roles should be cast.
        /// Each array in the sequence should be cast as one step, ie. a single element array recommends
        /// a call to <see cref="PickCast(IEnumerable{Applicant}, Role)"/>, whereas a multi-element array
        /// recommends a call to <see cref="BalanceCast(IEnumerable{Applicant}, IEnumerable{Role})"/>.
        /// This should return all roles in the show exactly once, regardless of whether or not they
        /// are already cast.</summary>
        IEnumerable<Role[]> IdealCastingOrder(ShowRoot show_root, Applicant[] applicants_in_cast);

        /// <summary>Calculate the suitability of an applicant for a role, regardless of availability and eligibility.
        /// Value returned will be between 0 and 1 (inclusive). This may contain logic specific to roles, and is therefore
        /// different to IAuditionEngine.SuitabilityOf(Applicant, Requirement).</summary>
        double SuitabilityOf(Applicant applicant, Role role);

        /// <summary>Count the number of roles an applicant has which require a certain criteria,
        /// optionally excluding a specified role. Depending on implementation, this may return a
        /// whole number of actual roles, or a sum of prorated roles based on the importance of
        /// the specified criteria to each role.</summary>
        double CountRoles(Applicant applicant, Criteria criteria, Role? excluding_role);

        /// <summary>Pick the best cast for a role, returning the chosen cast without casting them.</summary>
        Task<List<Applicant>> PickCast(IEnumerable<Applicant> applicants, Role role);

        /// <summary>A callback for when the user allocates cast to a role, providing
        /// information to the engine which can be used to improve future recommendations.</summary>
        Task UserPickedCast(IEnumerable<Applicant> applicants_picked, IEnumerable<Applicant> applicants_not_picked, Role role);

        /// <summary>Saves any changes made in the engine back to the ShowModel or otherwise.
        /// This should be called whenever the engine's job is finished.</summary>
        Task ExportChanges();

        /// <summary>Allocate the best cast to one or more roles, balancing talent between them.
        /// NOTE: Unlike <see cref="PickCast(IEnumerable{Applicant}, Role)"/> this directly allocates
        /// cast to the role rather than returning the chosen cast.</summary>
        Task BalanceCast(IEnumerable<Applicant> applicants, IEnumerable<Role> roles);

        /// <summary>Determine if an applicant is eligible to be cast in a role
        /// (ie. whether all minimum requirements of the role are met)</summary>
        Eligibility EligibilityOf(Applicant applicant, Role role);

        /// <summary>Determine if an applicant is available to be cast in a role
        /// (eg. already cast in the same item, an adjacent item, or within a section where AllowMultipleRoles==FALSE)</summary>
        Availability AvailabilityOf(Applicant applicant, Role role);
    }
}
