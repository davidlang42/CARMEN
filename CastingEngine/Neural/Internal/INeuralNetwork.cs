using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural.Internal
{
    public interface INeuralNetwork
    {
        public int InputCount { get; }
        public int OutputCount { get; }
        public double LearningRate { get; set; }

        public double Train(double[] inputs, double[] expected_outputs);

        public double[] Predict(double[] inputs);

        public string ToString();
    }
}
