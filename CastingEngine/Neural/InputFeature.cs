namespace Carmen.CastingEngine.Neural
{
    public class InputFeature
    {
        public double Calculate(double input) => input * Weight;
        public double Weight { get; set; }
    }
}
