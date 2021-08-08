using System;
using System.Collections.Generic;
using ShowModel;
using ShowModel.Applicants;
using ShowModel.Criterias;
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
        //LATER int OverallAbility(Applicant applicant);

        //TODO analyse distribution of sets of marks
        #endregion

        #region Cast selection
        /// <summary>Select applicants into a cast group</summary>
        //TODO IEnumerable<Applicant> SelectCastGroup(IEnumerable<Applicant> applicants, CastGroup cast_group);//TODO

        //TODO apply tags to cast

        //TODO set cast numbers/identifiers

        //TODO handle alternative casts
        //- need to have buddies
        //- need to match casting between buddies(by default but with exceptions?)
        //- need to be able to "swap" buddies between casts
        //- need to lock sets of applicants to be in the same cast(across cast groups, eg.junior boy, junior girl)
        //- need to be able to balance talent between casts
        //- need to have counts by group same for both casts
        #endregion

        #region Role allocation
        /// <summary>Determine the recommended order in which the roles should be cast</summary>
        IEnumerable<Role> CastingOrder(IEnumerable<Item> items_in_order);

        Role? NextRoleToCast(IEnumerable<Item> items_in_order, Role? excluding_role = null)
        {
            var e = CastingOrder(items_in_order).GetEnumerator();
            if (!e.MoveNext())
                return null;
            if (e.Current == excluding_role)
                if (!e.MoveNext())
                    return null;
            return e.Current;
        }

        /// <summary>Calculate the suitability of an applicant for a role</summary>
        double SuitabilityOf(Applicant applicant, Role role);

        /// <summary>Count the number of roles an applicant has which require a certain criteria, optionally excluding one role</summary>
        double CountRoles(Applicant applicant, Criteria criteria, Role? excluding_role);

        /// <summary>Determine if an applicant is available to be cast in a role
        /// (eg. already cast in the same item, an adjacent item, or within a section where AllowMultipleRoles==FALSE)</summary>
        Availability AvailabilityOf(Applicant applicant, Role role);

        /// <summary>Pick the cast for a role</summary>
        IEnumerable<Applicant> PickCast(IEnumerable<Applicant> applicants, Role role, IEnumerable<AlternativeCast> alternative_casts);

        /// <summary>Pick the cast for one or more roles, balancing talent between them</summary>
        //TODO Dictionary<Role, IEnumerable<Applicant>> PickCast(IEnumerable<Applicant> applicants, IEnumerable<Role> roles);
        #endregion
    }
}
