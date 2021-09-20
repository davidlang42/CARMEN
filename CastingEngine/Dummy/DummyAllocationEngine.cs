using Carmen.CastingEngine.Allocation;
using Carmen.CastingEngine.Base;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using System.Collections.Generic;
using System.Linq;

namespace Carmen.CastingEngine.Dummy
{
    /// <summary>
    /// For testing only, does not nessesarily produce valid casting, only valid at a data model/type level.
    /// </summary>
    public class DummyAllocationEngine : AllocationEngine
    {
        public DummyAllocationEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts)
            : base(applicant_engine, alternative_casts)
        { }

        /// <summary>Dummy value is the basic implementation of recursive requirements.
        /// This assumes there are no circular references.</summary>
        public override double SuitabilityOf(Applicant applicant, Role role)
        {
            var sub_suitabilities = role.Requirements.Select(req => ApplicantEngine.SuitabilityOf(applicant, req)).DefaultIfEmpty();
            return sub_suitabilities.Average();
        }

        /// <summary>Dummy selection picks the number of required applicants of each type from the list of applicants, ignoring availability and eligibility.
        /// This does not take into account people already cast in the role.</summary>
        public override IEnumerable<Applicant> PickCast(IEnumerable<Applicant> applicants, Role role)
        {
            foreach (var cbg in role.CountByGroups)
            {
                if (cbg.CastGroup.AlternateCasts)
                {
                    foreach (var cast in alternativeCasts)
                        foreach (var applicant in applicants.Where(a => a.CastGroup == cbg.CastGroup && a.AlternativeCast == cast).Take((int)cbg.Count))
                            yield return applicant;
                }
                else
                {
                    foreach (var applicant in applicants.Where(a => a.CastGroup == cbg.CastGroup).Take((int)cbg.Count))
                        yield return applicant;
                }
            }
        }
    }
}
