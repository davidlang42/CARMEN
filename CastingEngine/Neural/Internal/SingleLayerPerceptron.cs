using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural.Internal
{
    /// <summary>
    /// A neural network consisting of a single layer of neurons from inputs to outputs.
    /// This is a very simple type of network, which is only capable of predicting data which is linearly separable.
    /// </summary>
    public class SingleLayerPerceptron : INeuralNetwork
    {
        ILossFunction loss = new MeanSquaredError();
        public Layer Layer;

        public int InputCount { get; init; }
        public int OutputCount { get; init; }
        public double LearningRate { get; set; } = 0.05;

        public SingleLayerPerceptron(int n_inputs, int n_outputs, IVectorActivationFunction? activation = null)
        {
            InputCount = n_inputs;
            OutputCount = n_outputs;
            Layer = new Layer(n_inputs, n_outputs, activation ?? new Sigmoid());
        }

        /// <summary>Train the model with a single set of inputs and expected outputs.
        /// Returns the total loss prior to back propogation.</summary>
        public double Train(double[] inputs, double[] expected_outputs)
        {
            // Calculation
            var out_o = Predict(inputs);
            // Back propogation (stochastic gradient descent)
            var dloss_douto = loss.Derivative(out_o, expected_outputs);
            Layer.Train(inputs, out_o, dloss_douto, LearningRate, out _);
            return loss.Calculate(dloss_douto);
        }

        public double[] Predict(double[] inputs) => Layer.Predict(inputs);

        public override string ToString() => $"Outputs: {Layer}";
    }
}