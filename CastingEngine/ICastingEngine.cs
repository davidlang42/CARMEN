using System;
using System.Collections.Generic;
using ShowModel;
using ShowModel.Applicants;
using ShowModel.Structure;

namespace CastingEngine
{
    /// <summary>
    /// WIP
    /// </summary>
    public interface ICastingEngine
    {
        #region Applicant abilities
        /// <summary>Calculate the overall ability of an applicant</summary>
        int OverallAbility(Applicant applicant);

        // analyse distribution of sets of marks
        #endregion

        #region Cast selection
        /// <summary>Select applicants into a cast group</summary>
        IEnumerable<Applicant> SelectCastGroup(IEnumerable<Applicant> applicants, CastGroup cast_group);
        
        // apply tags to cast
        
        // set cast numbers/identifiers

        // handle alternative casts
        //- need to have buddies
        //- need to match casting between buddies(by default but with exceptions?)
        //- need to be able to "swap" buddies between casts
        //- need to lock sets of applicants to be in the same cast(across cast groups, eg.junior boy, junior girl)
        //- need to be able to balance talent between casts
        //- need to have counts by group same for both casts
        #endregion

        #region Role allocation
        /// <summary>Determine the recommended order in which the roles should be cast</summary>
        IEnumerable<Role> CastingOrder(IEnumerable<Item> items_in_order, IEnumerable<Role> roles);

        /// <summary>Calculate the suitability of an applicant for a role</summary>
        double SuitabilityOf(Applicant applicant, Role role);

        /// <summary>Determine if an applicant is available to be cast in a role
        /// (eg. already cast in the same item, an adjacent item, or within a section where AllowMultipleRoles==FALSE)</summary>
        bool AvailabilityOf(Applicant applicant, Role role);

        /// <summary>Pick the cast for a role</summary>
        IEnumerable<Applicant> PickCast(IEnumerable<Applicant> applicants, Role role);

        /// <summary>Pick the cast for one or more roles, balancing talent between them</summary>
        Dictionary<Role, IEnumerable<Applicant>> PickCast(IEnumerable<Applicant> applicants, IEnumerable<Role> roles);
        #endregion
    }
}
