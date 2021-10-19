using System.Collections.Generic;
using System.Linq;

namespace Carmen.CastingEngine.Neural
{
    public struct Montage
    {
        public bool Success;
        public bool ReachedMaxIterations;
        public bool ReachedStableLoss;
        public bool ContainsNaN;
        public double[] InitialLoss;
        public double[] FinalLoss;
        public int Iterations;
        public List<string> Descriptions;

        public double InitialAverageLoss => InitialLoss.DefaultIfEmpty().Average();
        public double FinalAverageLoss => FinalLoss.Average();
    }
}
