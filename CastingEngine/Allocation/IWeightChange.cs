using Carmen.ShowModel;

namespace Carmen.CastingEngine.Allocation
{
    public interface IWeightChange : IOrdered
    {
        protected const double MINIMUM_CHANGE = 0.1;

        public string Description { get; }
        public bool Significant { get; }
        public void Accept();
    }
}
