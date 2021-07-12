﻿using Model.Applicants;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Model.Requirements
{
    public abstract class CombinedRequirement : Requirement //TODO detect circular references in combined requirements & not requirements
    {
        public virtual ICollection<Requirement> SubRequirements { get; private set; } = new ObservableCollection<Requirement>();

        public CombinedRequirement(params Requirement[] requirements)
        {
            foreach (var requirement in requirements)
                SubRequirements.Add(requirement);
        }

        public abstract override double SuitabilityOf(Applicant applicant);
    }

    public class AndRequirement : CombinedRequirement
    {
        /// <summary>If false, Suitability will the product of the SubRequirement suitabilities</summary>
        public bool AverageSuitability { get; set; }

        public override bool IsSatisfiedBy(Applicant applicant)
            => SubRequirements.All(r => r.IsSatisfiedBy(applicant));

        public override double SuitabilityOf(Applicant applicant)
        {
            var sub_suitabilities = SubRequirements.Select(r => r.SuitabilityOf(applicant));
            return AverageSuitability ? sub_suitabilities.Average() : sub_suitabilities.Product();
        }
    }

    public class OrRequirement : CombinedRequirement
    {
        /// <summary>If false, Suitability will the maximum of the SubRequirement suitabilities</summary>
        public bool AverageSuitability { get; set; }

        public override bool IsSatisfiedBy(Applicant applicant)
            => SubRequirements.Any(r => r.IsSatisfiedBy(applicant));

        public override double SuitabilityOf(Applicant applicant)
        {
            var sub_suitabilities = SubRequirements.Select(r => r.SuitabilityOf(applicant));
            return AverageSuitability ? sub_suitabilities.Average() : sub_suitabilities.Max();
        }
    }

    public class XorRequirement : CombinedRequirement
    {
        public override bool IsSatisfiedBy(Applicant applicant)
            => SubRequirements.Where(r => r.IsSatisfiedBy(applicant)).Count() == 1;

        public override double SuitabilityOf(Applicant applicant)
            => SubRequirements.Where(r => r.IsSatisfiedBy(applicant)).SingleOrDefaultSafe()?.SuitabilityOf(applicant) ?? 0;
    }
}
