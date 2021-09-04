using Carmen.ShowModel.Structure;

namespace Carmen.CastingEngine.Allocation
{
    public struct NonMultiSectionItem
    {
        public Section NonMultiSection { get; init; }
        public Item AlreadyInItem { get; init; }
    }
}
