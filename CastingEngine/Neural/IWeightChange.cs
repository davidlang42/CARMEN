using Carmen.ShowModel;

namespace Carmen.CastingEngine.Neural
{
    public interface IWeightChange
    {
        protected const double MINIMUM_CHANGE = 0.1;

        public IOrdered Requirement { get; }
        public string Description { get; }
        public bool Significant { get; }
        public void Accept();
    }
}
