using ShowModel.Applicants;

namespace ShowModel.Requirements
{
    public class CastGroupRequirement : Requirement //TODO detect circular dependencies when using CastGroupRequirements on CastGroups
    {
        internal int RequiredGroupId { get; set; } // DbSet.Load() throws IndexOutOfRangeException if foreign key is not defined
        public virtual CastGroup RequiredGroup { get; set; } = null!;

        public override bool IsSatisfiedBy(Applicant applicant)
            => applicant.CastGroups.Contains(RequiredGroup);
    }
}
