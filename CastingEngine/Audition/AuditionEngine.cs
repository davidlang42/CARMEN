using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Carmen.CastingEngine.Audition
{
    /// <summary>
    /// The abstract base class of most IAuditionEngine based engines
    /// </summary>
    public abstract class AuditionEngine : IAuditionEngine
    {
        /// <summary>A list of available audition engines</summary>
        public static readonly Type[] Implementations = new[] {
            typeof(NeuralAuditionEngine),
            typeof(WeightedSumEngine),
        };

        public abstract int MaxOverallAbility { get; }
        public abstract int MinOverallAbility { get; }

        readonly FunctionCache<Applicant, int> overallAbility = new(applicant
            => Convert.ToInt32(applicant.Abilities.Sum(a => (double)a.Mark / a.Criteria.MaxMark * a.Criteria.Weight)));

        /// <summary>Calculate the overall ability of an Applicant as a weighted sum of their Abilities.
        /// NOTE: This is cached for speed, as an Applicant's abilities shouldn't change over the lifetime of an AuditionEngine</summary>
        public int OverallAbility(Applicant applicant) => overallAbility[applicant];

        public virtual void UserSelectedCast(IEnumerable<Applicant> applicants_accepted, IEnumerable<Applicant> applicants_rejected)
        { }

        readonly FunctionCache<Applicant, Requirement, double> suitabilityOf = new();
        /// <summary>Assumes no circular references between requirements
        /// NOTE: This is cached for speed, as an Applicant's abilities and Requirement specifications shouldn't change over the lifetime of an AuditionEngine</summary>
        public double SuitabilityOf(Applicant applicant, Requirement requirement) => suitabilityOf.Get(applicant, requirement, (applicant, requirement)
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
            });
    }
}
