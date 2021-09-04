using Carmen.ShowModel.Structure;

namespace Carmen.CastingEngine.Allocation
{
    public struct AdjacentItem
    {
        public Item AlreadyInItem { get; init; }
        public Adjacency Adjacency { get; init; }
        public Item AdjacentTo { get; init; }
        public InnerNode NonConsecutiveSection { get; init; }
    }
}
