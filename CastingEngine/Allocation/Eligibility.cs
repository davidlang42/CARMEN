using Carmen.ShowModel.Requirements;

namespace Carmen.CastingEngine.Allocation
{
    public struct Eligibility
    {
        public Requirement[] RequirementsNotMet { get; init; }

        public bool IsEligible => RequirementsNotMet.Length == 0;
    }
}
