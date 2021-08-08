using System;
using System.Collections.Generic;
using System.Linq;
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
        //TODO int OverallAbility(Applicant applicant);
        #endregion

        #region Cast selection
        /// <summary>Select applicants into cast groups, respecting those already placed
        /// NOTE: CastGroup requirements may not depend on CastGroups or Tags</summary>
        void SelectCastGroups(IEnumerable<Applicant> applicants, IEnumerable<CastGroup> cast_groups);//TODO CALL

        /// <summary>Balance applicants between alternative casts, respecting those already set
        /// NOTE: All applicants must have a CastGroup set</summary>
        void BalanceAlternativeCasts(IEnumerable<Applicant> applicants, IEnumerable<AlternativeCast> alternative_casts, IEnumerable<SameCastSet> same_cast_sets);//TODO CALL

        /// <summary>Allocate cast numbers, respecting those already set
        /// NOTE: All applicants must have a CastGroup (and AlternativeCast when CastGroup.AlternateCasts) set</summary>
        void AllocateCastNumbers(IEnumerable<Applicant> applicants, Criteria order_by);//TODO CALL

        /// <summary>Apply tags to applicants, respecting those already applied
        /// NOTE: Tag requirements may depend on other Tags, as long as there is no circular
        /// dependency and that tag is also being applied as part of this call</summary>
        void ApplyTags(IEnumerable<Applicant> applicants, IEnumerable<Tag> tags);//TODO CALL
        #endregion

        //TODO revise role allocation section
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
