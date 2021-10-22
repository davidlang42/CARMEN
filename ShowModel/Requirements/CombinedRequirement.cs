using Carmen.ShowModel.Applicants;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Carmen.ShowModel.Requirements
{
    public abstract class CombinedRequirement : Requirement
    {
        private ObservableCollection<Requirement> subRequirements = new();
        public virtual ICollection<Requirement> SubRequirements => subRequirements;

        public CombinedRequirement(params Requirement[] requirements)
        {
            foreach (var requirement in requirements)
                SubRequirements.Add(requirement);
        }

        internal override bool HasCircularReference(HashSet<Requirement> path, HashSet<Requirement> visited)
        {
            visited.Add(this);
            if (!path.Add(this))
                return true;
            if (SubRequirements.Any(sr => sr.HasCircularReference(path, visited)))
                return true;
            path.Remove(this);
            return false;
        }

        public override IEnumerable<string> Validate()
        {
            if (SubRequirements.Count == 0)
                yield return $"Requirement '{Name}' has no sub requirements.";
            foreach (var base_issue in base.Validate())
                yield return base_issue;
        }
    }

    public class AndRequirement : CombinedRequirement
    {
        private bool averageSuitability;
        /// <summary>If false, Suitability will the product of the SubRequirement suitabilities</summary>
        public bool AverageSuitability
        {
            get => averageSuitability;
            set
            {
                if (averageSuitability == value)
                    return;
                averageSuitability = value;
                OnPropertyChanged();
            }
        }

        public override bool IsSatisfiedBy(Applicant applicant)
            => SubRequirements.All(r => r.IsSatisfiedBy(applicant));

        public override bool IsSatisfiedBy(Applicant applicant, ICollection<Requirement> accumulate_failed_requirements)
        {
            var result = true;
            foreach (var req in SubRequirements)
            {
                if (!req.IsSatisfiedBy(applicant, accumulate_failed_requirements))
                    result = false;
            }
            return result;
        }
    }

    public class OrRequirement : CombinedRequirement
    {
        private bool averageSuitability;
        /// <summary>If false, Suitability will the maximum of the SubRequirement suitabilities</summary>
        public bool AverageSuitability
        {
            get => averageSuitability;
            set
            {
                if (averageSuitability == value)
                    return;
                averageSuitability = value;
                OnPropertyChanged();
            }
        }

        public override bool IsSatisfiedBy(Applicant applicant)
            => SubRequirements.Any(r => r.IsSatisfiedBy(applicant));
    }

    public class XorRequirement : CombinedRequirement
    {
        public override bool IsSatisfiedBy(Applicant applicant)
            => SubRequirements.Where(r => r.IsSatisfiedBy(applicant)).Count() == 1;
    }
}
