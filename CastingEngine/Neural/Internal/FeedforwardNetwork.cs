using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural.Internal
{
    /// <summary>
    /// A neural network similar to a MultiLayerPerceptron, except with an arbitrary number of hidden layers between inputs and outputs.
    /// Increasing the number and width of hidden layers allows for modelling more complex data, but beware of overfitting.
    /// </summary>
    public class FeedforwardNetwork : INeuralNetwork
    {
        Layer[] layers;

        public int InputCount { get; init; }
        public int OutputCount { get; init; }
        public double LearningRate { get; set; } = 0.05;
        public ILossFunction LossFunction { get; set; }

        /// <summary>Create a feedforward neural network, with initially random weights and biases, based on the structural
        /// parameters provided</summary>
        /// <param name="n_inputs">The number of neurons in the input layer (raw input values)</param>
        /// <param name="n_hidden_layers">The number of hidden layers of neurons between the input and output layers</param>
        /// <param name="n_neurons_per_hidden_layer">A function which returns the number of neurons for a given layer index (0 to n_hidden_layers - 1)</param>
        /// <param name="n_outputs">The number of neurons in the output layer (predicted output values)</param>
        /// <param name="hidden_layer_activation">The activation function to use for all hidden layers (defaults to Tanh)</param>
        /// <param name="output_layer_activation">The activation function to use for the output layer (defaults to Sigmoid)</param>
        public FeedforwardNetwork(int n_inputs, int n_hidden_layers, Func<int,int> n_neurons_per_hidden_layer, int n_outputs,
            IVectorActivationFunction? hidden_layer_activation = null, IVectorActivationFunction? output_layer_activation = null)
        {
            LossFunction = new MeanSquaredError();
            InputCount = n_inputs;
            OutputCount = n_outputs;
            if (n_hidden_layers < 0)
                throw new ArgumentException($"{nameof(n_hidden_layers)} must be greater than or equal to 0");
            hidden_layer_activation ??= new Tanh();
            output_layer_activation ??= new Sigmoid();
            layers = new Layer[n_hidden_layers + 1];
            for (var i = 0; i < n_hidden_layers; i++)
            {
                var n_neurons = n_neurons_per_hidden_layer(i);
                layers[i] = new Layer(n_inputs, n_neurons, hidden_layer_activation);
                n_inputs = n_neurons;
            }
            layers[n_hidden_layers] = new Layer(n_inputs, n_outputs, output_layer_activation);
        }

        /// <summary>Create a feedforward neural network, with initially random weights and biases, based on the structural
        /// parameters provided</summary>
        /// <param name="n_inputs">The number of neurons in the input layer (raw input values)</param>
        /// <param name="n_hidden_layers">The number of hidden layers of neurons between the input and output layers</param>
        /// <param name="n_neurons_per_hidden_layer">The number of neurons in each hidden layer</param>
        /// <param name="n_outputs">The number of neurons in the output layer (predicted output values)</param>
        /// <param name="hidden_layer_activation">The activation function to use for all hidden layers (defaults to Tanh)</param>
        /// <param name="output_layer_activation">The activation function to use for the output layer (defaults to Sigmoid)</param>
        public FeedforwardNetwork(int n_inputs, int n_hidden_layers, int n_neurons_per_hidden_layer, int n_outputs,
            IVectorActivationFunction? hidden_layer_activation = null, IVectorActivationFunction? output_layer_activation = null)
            : this(n_inputs, n_hidden_layers, i => n_neurons_per_hidden_layer, n_outputs, hidden_layer_activation, output_layer_activation)
        { }

        /// <summary>Train the model with a single set of inputs and expected output.
        /// Returns the total loss prior to back propogation.</summary>
        public double Train(double[] inputs, double[] expected_outputs)
        {
            // Calculation
            var out_o = new double[layers.Length][];
            out_o[0] = layers[0].Predict(inputs);
            for (var i = 1; i < layers.Length; i++)
                out_o[i] = layers[i].Predict(out_o[i - 1]);
            // Back propogation (stochastic gradient descent)
            var dloss_douto = LossFunction.Derivative(out_o[out_o.Length - 1], expected_outputs);
            var total_loss = LossFunction.Calculate(dloss_douto);
            for (var i = out_o.Length - 1; i > 0; i--)
            {
                layers[i].Train(out_o[i - 1], out_o[i], dloss_douto, LearningRate, out var dloss_dino);
                // Prep for next layer
                dloss_douto = new double[layers[i - 1].Neurons.Length];
                for (var h = 0; h < dloss_douto.Length; h++)
                    for (var n = 0; n < layers[i].Neurons.Length; n++)
                        dloss_douto[h] += dloss_dino[n] * layers[i].Neurons[n].Weights[h];
            }
            layers[0].Train(inputs, out_o[0], dloss_douto, LearningRate, out _);
            return total_loss;
        }

        public double[] Predict(double[] inputs)
        {
            double[] out_o = inputs;
            for (var i = 0; i < layers.Length; i++)
                out_o = layers[i].Predict(out_o);
            return out_o;
        }

        public override string ToString()
        {
            var s = "";
            if (layers.Length == 2)
                s += $"Hidden: {string.Join(" / ", layers[0])}; ";
            else
                for (var i = 0; i < layers.Length - 1; i++)
                    s += $"Hidden{i}: {string.Join(" / ", layers[0])}; ";
            s += $"Output: {string.Join(" / ", layers[layers.Length - 1])};";
            return s;
        }
    }
}
