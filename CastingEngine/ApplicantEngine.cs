using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine
{
    /// <summary>
    /// The abstract base class of most IApplicantEngine based engines
    /// </summary>
    public abstract class ApplicantEngine : IApplicantEngine
    {
        public abstract int MaxOverallAbility { get; }
        public abstract int MinOverallAbility { get; }

        public abstract int OverallAbility(Applicant applicant);

        /// <summary>Assumes no circular references between requirements</summary>
        public virtual double SuitabilityOf(Applicant applicant, Requirement requirement)
            => requirement switch
            {
                AbilityRangeRequirement arr when arr.ScaleSuitability => applicant.MarkFor(arr.Criteria) / (double)arr.Criteria.MaxMark,
                NotRequirement nr => 1 - SuitabilityOf(applicant, nr.SubRequirement),
                AndRequirement ar => ar.AverageSuitability
                    ? ar.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Average()
                    : ar.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Product(),
                OrRequirement or => or.AverageSuitability
                    ? or.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Average()
                    : or.SubRequirements.Select(r => SuitabilityOf(applicant, r)).Max(),
                XorRequirement xr => xr.SubRequirements.Where(r => r.IsSatisfiedBy(applicant)).SingleOrDefaultSafe() is Requirement req ? SuitabilityOf(applicant, req) : 0,
                Requirement req => req.IsSatisfiedBy(applicant) ? 1 : 0
            };
    }
}
