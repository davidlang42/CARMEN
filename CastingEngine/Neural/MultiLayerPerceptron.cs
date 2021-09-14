using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    /// <summary>
    /// A neural network similar to a SingleLayerPerceptron, except with exactly 1 hidden layer between inputs and outputs.
    /// The hidden layer allows prediction of non-linearly separable data.
    /// </summary>
    public class MultiLayerPerceptron
    {
        ILossFunction loss = new MeanSquaredError();
        Layer hidden, output;
        double learningRate = 0.05;

        public MultiLayerPerceptron(int n_inputs, int n_hidden_layer_neurons, int n_outputs)
        {
            hidden = new Layer(n_inputs, n_hidden_layer_neurons, new Tanh());
            output = new Layer(n_hidden_layer_neurons, n_outputs, new Sigmoid());
        }

        /// <summary>Train the model with a single set of inputs and expected output.
        /// Returns the total loss prior to back propogation.</summary>
        public double Train(double[] inputs, double[] expected_outputs)
        {
            // Calculation
            var out_o1 = hidden.Predict(inputs);
            var out_o2 = output.Predict(out_o1);
            // Back propogation (stochastic gradient descent)
            var dloss_douto2 = loss.Derivative(out_o2, expected_outputs);
            output.Train(out_o1, out_o2, dloss_douto2, learningRate, out var dloss_dino2);
            // Next layer
            var dloss_douto1 = new double[hidden.Neurons.Length];
            for (var h = 0; h < hidden.Neurons.Length; h++)
                for (var n = 0; n < output.Neurons.Length; n++)
                    dloss_douto1[h] += dloss_dino2[n] * output.Neurons[n].Weights[h];
            hidden.Train(inputs, out_o1, dloss_douto1, learningRate, out _);
            return loss.Calculate(dloss_douto2);
        }

        public double[] Predict(double[] inputs)
        {
            var result = hidden.Predict(inputs);
            result = output.Predict(result);
            return result;
        }

        public override string ToString() => $"Hidden: {string.Join(" / ", hidden)}; Output: {string.Join(" / ", output)};";
    }
}
