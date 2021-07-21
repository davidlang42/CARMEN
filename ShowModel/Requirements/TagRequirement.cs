using ShowModel.Applicants;

namespace ShowModel.Requirements
{
    public class TagRequirement : Requirement //TODO detect circular dependencies when using CastGroupRequirements on CastGroups
    {
        internal int RequiredTagId { get; set; } // DbSet.Load() throws IndexOutOfRangeException if foreign key is not defined
        public virtual Tag RequiredTag { get; set; } = null!;

        public override bool IsSatisfiedBy(Applicant applicant)
            => applicant.Tags.Contains(RequiredTag);
    }
}
