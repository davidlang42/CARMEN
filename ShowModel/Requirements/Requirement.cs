using ShowModel.Applicants;
using ShowModel.Structure;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ShowModel.Requirements
{
    /// <summary>
    /// A requirement which can be satisfied by an Applicant
    /// </summary>
    public abstract class Requirement : IOrdered
    {
        #region Database fields
        [Key]
        public int RequirementId { get; private set; }
        public string Name { get; set; } = "";
        public int Order { get; set; }
        public bool Primary { get; set; }
        public string? Reason { get; set; }
        public virtual ICollection<Role> UsedByRoles { get; set; } = new ObservableCollection<Role>();
        public virtual ICollection<CastGroup> UsedByCastGroups { get; set; } = new ObservableCollection<CastGroup>();
        public virtual ICollection<CombinedRequirement> UsedByCombinedRequirements { get; set; } = new ObservableCollection<CombinedRequirement>();
        public virtual ICollection<Identifier> UsedByIdentifiers { get; set; } = new ObservableCollection<Identifier>();
        #endregion

        /// <summary>Calculates the suitability of an Applicant.
        /// Value returned will be between 0 and 1 (inclusive).</summary>
        public virtual double SuitabilityOf(Applicant applicant)
            => IsSatisfiedBy(applicant) ? 1 : 0;

        /// <summary>Checks if an Applicant satisfies this requirement.</summary>
        public abstract bool IsSatisfiedBy(Applicant applicant);
    }
}
