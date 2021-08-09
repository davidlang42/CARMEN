using ShowModel.Applicants;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ShowModel.Requirements
{
    public abstract class CombinedRequirement : Requirement //LATER implement INotifyPropertyChanged for completeness
    {
        public virtual ICollection<Requirement> SubRequirements { get; private set; } = new ObservableCollection<Requirement>();

        public CombinedRequirement(params Requirement[] requirements)
        {
            foreach (var requirement in requirements)
                SubRequirements.Add(requirement);
        }

        internal override bool HasCircularReference(HashSet<Requirement> visited)
        {
            if (!visited.Add(this))
                return true;
            if (SubRequirements.Any(sr => sr.HasCircularReference(visited)))
                return true;
            visited.Remove(this);
            return false;
        }
    }

    public class AndRequirement : CombinedRequirement //LATER implement INotifyPropertyChanged for completeness
    {
        /// <summary>If false, Suitability will the product of the SubRequirement suitabilities</summary>
        public bool AverageSuitability { get; set; }

        public override bool IsSatisfiedBy(Applicant applicant)
            => SubRequirements.All(r => r.IsSatisfiedBy(applicant));
    }

    public class OrRequirement : CombinedRequirement //LATER implement INotifyPropertyChanged for completeness
    {
        /// <summary>If false, Suitability will the maximum of the SubRequirement suitabilities</summary>
        public bool AverageSuitability { get; set; }

        public override bool IsSatisfiedBy(Applicant applicant)
            => SubRequirements.Any(r => r.IsSatisfiedBy(applicant));
    }

    public class XorRequirement : CombinedRequirement
    {
        public override bool IsSatisfiedBy(Applicant applicant)
            => SubRequirements.Where(r => r.IsSatisfiedBy(applicant)).Count() == 1;
    }
}
