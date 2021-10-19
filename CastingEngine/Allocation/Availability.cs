using Carmen.ShowModel.Structure;

namespace Carmen.CastingEngine.Allocation
{
    public struct Availability
    {
        public Item[] AlreadyInItems { get; init; }
        public NonMultiSectionItem[] AlreadyInNonMultiSections { get; init; }
        public AdjacentItem[] InAdjacentItems { get; init; }

        public bool IsAvailable => !IsAlreadyInItem && !IsAlreadyInNonMultiSection && !IsInAdjacentItem;
        public bool IsAlreadyInItem => AlreadyInItems?.Length > 0;
        public bool IsAlreadyInNonMultiSection => AlreadyInNonMultiSections?.Length > 0;
        public bool IsInAdjacentItem => InAdjacentItems?.Length > 0;
    }
}
