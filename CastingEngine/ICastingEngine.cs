using System;
using System.Collections.Generic;
using ShowModel;
using ShowModel.Applicants;
using ShowModel.Structure;

namespace CastingEngine
{
    public interface ICastingEngine//TODO WIP
    {
        #region Applicant abilities
        /// <summary>Calculate the overall ability of an applicant</summary>
        int OverallAbility(Applicant applicant);
        //TODO analyse distribution of sets of marks
        #endregion

        #region Cast selection
        /// <summary>Select applicants into a cast group</summary>
        IEnumerable<Applicant> SelectCastGroup(IEnumerable<Applicant> applicants, CastGroup cast_group);
        //TODO apply tags to cast
        //TODO set cast numbers/identifiers
        #endregion

        #region Role allocation
        /// <summary>Determine the recommended order in which the roles should be cast</summary>
        IEnumerable<Role> CastingOrder(IEnumerable<Item> items_in_order, IEnumerable<Role> roles);

        /// <summary>Calculate the suitability of an applicant for a role</summary>
        double SuitabilityOf(Applicant applicant, Role role);

        /// <summary>Determine if an applicant is available to be cast in a role
        /// (eg. already cast in the same item, an adjacent item, or within a section where AllowMultipleRoles==FALSE)</summary>
        bool AvailabilityOf(Applicant applicant, Role role);//TODO somehow (enum? string?) return why they aren't available

        /// <summary>Pick the cast for a role</summary>
        IEnumerable<Applicant> PickCast(IEnumerable<Applicant> applicants, Role role);

        /// <summary>Pick the cast for one or more roles, balancing talent between them</summary>
        Dictionary<Role, IEnumerable<Applicant>> PickCast(IEnumerable<Applicant> applicants, IEnumerable<Role> roles);
        #endregion
    }
}
