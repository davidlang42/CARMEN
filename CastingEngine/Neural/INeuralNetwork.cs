using System.Collections.Generic;

namespace Carmen.CastingEngine.Neural
{
    public interface INeuralNetwork
    {
        public IEnumerable<Layer> Layers { get; }
        public int InputCount { get; }
        public int OutputCount { get; }
        public double LearningRate { get; set; }
        public LossFunctionChoice LossFunction { get; set; }

        public double Train(double[] inputs, double[] expected_outputs);

        public double[] Predict(double[] inputs);

        public string ToString();
    }
}
