using ShowModel.Applicants;
using ShowModel.Structure;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ShowModel.Requirements
{
    /// <summary>
    /// A requirement which can be satisfied by an Applicant
    /// </summary>
    public abstract class Requirement : IOrdered, IValidatable
    {
        #region Database fields
        [Key]
        public int RequirementId { get; private set; }
        public string Name { get; set; } = "";
        public int Order { get; set; }
        public bool Primary { get; set; }
        public string? Reason { get; set; }
        public virtual ICollection<Role> UsedByRoles { get; private set; } = new ObservableCollection<Role>();
        public virtual ICollection<CastGroup> UsedByCastGroups { get; private set; } = new ObservableCollection<CastGroup>();
        public virtual ICollection<CombinedRequirement> UsedByCombinedRequirements { get; private set; } = new ObservableCollection<CombinedRequirement>();
        public virtual ICollection<Identifier> UsedByIdentifiers { get; private set; } = new ObservableCollection<Identifier>();
        public virtual ICollection<Tag> UsedByTags { get; private set; } = new ObservableCollection<Tag>();
        #endregion

        /// <summary>Calculates the suitability of an Applicant.
        /// Value returned will be between 0 and 1 (inclusive).</summary>
        public virtual double SuitabilityOf(Applicant applicant)
            => IsSatisfiedBy(applicant) ? 1 : 0;

        /// <summary>Checks if an Applicant satisfies this requirement.</summary>
        public abstract bool IsSatisfiedBy(Applicant applicant);

        internal virtual bool HasCircularReference(HashSet<Requirement> visited) => false;

        /// <summary>Checks if this requirement has a circular reference.</summary>
        public IEnumerable<string> Validate()
        {
            var visited = new HashSet<Requirement>();
            if (HasCircularReference(visited))
                yield return $"Requirement has a circular reference: ({string.Join(", ", visited.Select(r => r.Name))})";
        }
    }
}
