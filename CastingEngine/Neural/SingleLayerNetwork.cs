using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    public class SingleLayerNetwork
    {
        ILossFunction loss = new MeanSquaredError();
        public Layer layer;
        double learningRate = 0.05;

        public SingleLayerNetwork(int n_inputs, int n_outputs)
        {
            layer = new Layer(n_inputs, n_outputs, new Sigmoid());
        }

        /// <summary>Train the model with a single set of inputs and expected outputs.
        /// Returns the total loss prior to back propogation.</summary>
        public double Train(double[] inputs, double[] expected_outputs)
        {
            // Calculation
            var out_o = Predict(inputs);
            // Back propogation (stochastic gradient descent)
            var dloss_douto = loss.Derivative(out_o, expected_outputs);
            layer.Train(inputs, out_o, dloss_douto, learningRate);
            return loss.Calculate(out_o, expected_outputs);
        }

        public double[] Predict(double[] inputs) => layer.Predict(inputs);

        public override string ToString() => $"Outputs: {layer}";
    }
}