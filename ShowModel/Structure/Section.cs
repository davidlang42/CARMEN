using ShowModel.Applicants;
using System.Collections.Generic;
using System.Linq;

namespace ShowModel.Structure
{
    /// <summary>
    /// A section of the show, containing items or other sections.
    /// </summary>
    public class Section : InnerNode //LATER implement INotifyPropertyChanged for completeness
    {
        public enum RolesMatchResult
        {
            RolesMatch,
            TooFewRoles,
            TooManyRoles
        }

        public virtual SectionType SectionType { get; set; } = null!;

        /// <summary>Check if the Roles in this Section sum meet the conditions of this SectionType.
        /// If SectionType.AllowNoRoles == false and sum of roles &lt; cast members for any CastGroup, this will return TooFewRoles.
        /// If SectionType.AllowMultipleRoles == false and sum of roles &gt; cast members for any CastGroup, this will return TooManyRoles.
        /// Otherwise, returns RolesMatch.</summary>
        public RolesMatchResult RolesMatchCastMembers(Dictionary<CastGroup, uint> cast_members)
        {
            if (SectionType.AllowNoRoles && SectionType.AllowMultipleRoles)
                return RolesMatchResult.RolesMatch; // nothing to check
            var sum_of_roles = cast_members.ToDictionary(cm => cm.Key, cm => 0u);
            foreach (var role in ItemsInOrder().SelectMany(i => i.Roles).Distinct())
                foreach (var role_cbg in role.CountByGroups)
                    sum_of_roles[role_cbg.CastGroup] += role_cbg.Count; //TODO what if a role has a countbygroup for a cast group with no cast members in it?
            if (SectionType.AllowNoRoles == false && sum_of_roles.Any(kvp => cast_members[kvp.Key] > kvp.Value))
                return RolesMatchResult.TooFewRoles;
            if (SectionType.AllowMultipleRoles == false && sum_of_roles.Any(kvp => cast_members[kvp.Key] < kvp.Value))
                return RolesMatchResult.TooManyRoles;
            return RolesMatchResult.RolesMatch;
        }

        /// <summary>Technically this verifies the same business logic as RolesMatchCastMembers(), but it checks the actual casting, rather than the sum of roles.
        /// The out parameters of cast counts are calculated only if they may break the SectionType rules.</summary>
        public bool CastingMeetsSectionTypeRules(int total_cast_members, out int cast_with_no_roles, out int cast_with_multiple_roles)
        {
            var roles_per_cast = ItemsInOrder()
                .SelectMany(i => i.Roles).Distinct()
                .SelectMany(r => r.Cast).GroupBy(a => a)
                .ToDictionary(g => g.Key, g => g.Count()); //LATER does this need await?
            if (SectionType.AllowNoRoles)
                cast_with_no_roles = 0;
            else 
                cast_with_no_roles = total_cast_members - roles_per_cast.Count;
            if (SectionType.AllowMultipleRoles)
                cast_with_multiple_roles = 0;
            else
                cast_with_multiple_roles = roles_per_cast.Values.Count(v => v > 1);
            return cast_with_no_roles == 0 && cast_with_multiple_roles == 0;
        }
    }
}
