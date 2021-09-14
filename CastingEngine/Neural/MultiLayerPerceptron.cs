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
    public class MultiLayerPerceptron //TODO make FeedforwardNetwork with an arbitrary number of hidden layers
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
            var out_o_hidden = hidden.Predict(inputs);
            var out_o = output.Predict(out_o_hidden);
            // Back propogation (stochastic gradient descent)
            var dloss_douto = loss.Derivative(out_o, expected_outputs);
            output.Train(inputs, out_o, dloss_douto, learningRate);
            // Next layer
            var dloss_douto_hidden = new double[hidden.Neurons.Length];
            for (var h = 0; h < hidden.Neurons.Length; h++)
                for (var n = 0; n < output.Neurons.Length; n++)
                    dloss_douto_hidden[h] += dloss_douto[n] * output.Neurons[n].Weights[h];
            hidden.Train(inputs, out_o_hidden, dloss_douto_hidden, learningRate);
            return loss.Calculate(out_o, expected_outputs);
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
