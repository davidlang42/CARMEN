using System;
using System.Collections.Generic;
using System.Linq;

namespace Carmen.CastingEngine.Neural
{
    /// <summary>
    /// A neural network similar to a MultiLayerPerceptron, except with an arbitrary number of hidden layers between inputs and outputs.
    /// Increasing the number and width of hidden layers allows for modelling more complex data, but beware of overfitting.
    /// </summary>
    public class FeedforwardNetwork : INeuralNetwork
    {
        readonly public Layer[] Layers; // only the array size is readonly
        IEnumerable<Layer> INeuralNetwork.Layers => Layers;

        public int InputCount => Layers.First().Neurons.First().InputCount;
        public int OutputCount => Layers.Last().NeuronCount;
        private double learningRate = 0.05;
        public double LearningRate
        {
            get => learningRate;
            set => learningRate = value;
        }

        private LossFunctionChoice lossFunction = LossFunctionChoice.MeanSquaredError;
        public LossFunctionChoice LossFunction
        {
            get => lossFunction;
            set
            {
                if (lossFunction == value)
                    return;
                lossFunction = value;
                loss = LossFunction.Create();
            }
        }

        private ILossFunction loss;

        /// <summary>Parameterless constructor for serialisation</summary>
        private FeedforwardNetwork()
        {
            loss = LossFunction.Create();
            Layers = Array.Empty<Layer>();
        }

        /// <summary>Create a feedforward neural network, with initially random weights and biases, based on the structural
        /// parameters provided</summary>
        /// <param name="n_inputs">The number of neurons in the input layer (raw input values)</param>
        /// <param name="n_hidden_layers">The number of hidden layers of neurons between the input and output layers</param>
        /// <param name="n_neurons_per_hidden_layer">A function which returns the number of neurons for a given layer index (0 to n_hidden_layers - 1)</param>
        /// <param name="n_outputs">The number of neurons in the output layer (predicted output values)</param>
        /// <param name="hidden_layer_activation">The activation function to use for all hidden layers (defaults to Tanh)</param>
        /// <param name="output_layer_activation">The activation function to use for the output layer (defaults to Sigmoid)</param>
        public FeedforwardNetwork(int n_inputs, int n_hidden_layers, Func<int,int> n_neurons_per_hidden_layer, int n_outputs,
            ActivationFunctionChoice hidden_layer_activation = ActivationFunctionChoice.Tanh,
            ActivationFunctionChoice output_layer_activation = ActivationFunctionChoice.Sigmoid)
        {
            loss = LossFunction.Create();
            if (n_hidden_layers < 0)
                throw new ArgumentException($"{nameof(n_hidden_layers)} must be greater than or equal to 0");
            Layers = new Layer[n_hidden_layers + 1];
            for (var i = 0; i < n_hidden_layers; i++)
            {
                var n_neurons = n_neurons_per_hidden_layer(i);
                Layers[i] = new Layer(n_inputs, n_neurons, hidden_layer_activation);
                n_inputs = n_neurons;
            }
            Layers[n_hidden_layers] = new Layer(n_inputs, n_outputs, output_layer_activation);
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
            ActivationFunctionChoice hidden_layer_activation = ActivationFunctionChoice.Tanh,
            ActivationFunctionChoice output_layer_activation = ActivationFunctionChoice.Sigmoid)
            : this(n_inputs, n_hidden_layers, i => n_neurons_per_hidden_layer, n_outputs, hidden_layer_activation, output_layer_activation)
        { }

        /// <summary>Train the model with a single set of inputs and expected output.
        /// Returns the loss prior to back propogation.</summary>
        public double Train(double[] inputs, double[] expected_outputs)
        {
            // Calculation
            var out_o = new double[Layers.Length][];
            out_o[0] = Layers[0].Predict(inputs);
            for (var i = 1; i < Layers.Length; i++)
                out_o[i] = Layers[i].Predict(out_o[i - 1]);
            // Back propogation (stochastic gradient descent)
            var dloss_douto = loss.Derivative(out_o[out_o.Length - 1], expected_outputs);
            var average_loss = loss.Calculate(dloss_douto, out_o[out_o.Length - 1], expected_outputs);
            for (var i = out_o.Length - 1; i > 0; i--)
            {
                var layer = Layers[i];
                layer.Train(out_o[i - 1], out_o[i], dloss_douto, learningRate, out var dloss_dino);
                // Prep for next layer
                dloss_douto = new double[Layers[i - 1].Neurons.Length];
                for (var n = 0; n < layer.Neurons.Length; n++)
                {
                    var dloss_dino_n = dloss_dino[n];
                    var neuron = layer.Neurons[n];
                    for (var h = 0; h < dloss_douto.Length; h++)
                        dloss_douto[h] += dloss_dino_n * neuron.Weights[h];
                }
            }
            Layers[0].Train(inputs, out_o[0], dloss_douto, learningRate);
            return average_loss;
        }

        public double[] Predict(double[] inputs)
        {
            double[] out_o = inputs;
            for (var i = 0; i < Layers.Length; i++)
                out_o = Layers[i].Predict(out_o);
            return out_o;
        }

        public override string ToString()
        {
            var s = "";
            if (Layers.Length == 2)
                s += $"Hidden: {string.Join(" / ", Layers[0])}; ";
            else
                for (var i = 0; i < Layers.Length - 1; i++)
                    s += $"Hidden{i}: {string.Join(" / ", Layers[0])}; ";
            s += $"Output: {string.Join(" / ", Layers[Layers.Length - 1])};";
            return s;
        }
    }
}
