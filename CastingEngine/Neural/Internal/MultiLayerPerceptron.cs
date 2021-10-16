using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural.Internal
{
    /// <summary>
    /// A neural network similar to a SingleLayerPerceptron, except with exactly 1 hidden layer between inputs and outputs.
    /// The hidden layer allows prediction of non-linearly separable data.
    /// </summary>
    public class MultiLayerPerceptron : INeuralNetwork
    {
        public Layer Hidden;
        public Layer Output;

        IEnumerable<Layer> INeuralNetwork.Layers
        {
            get
            {
                yield return Hidden;
                yield return Output;
            }
        }

        public int InputCount => Hidden.Neurons.First().InputCount;
        public int OutputCount => Output.NeuronCount;
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

        public MultiLayerPerceptron(int n_inputs, int n_hidden_layer_neurons, int n_outputs,
            ActivationFunctionChoice hidden_layer_activation = ActivationFunctionChoice.Tanh,
            ActivationFunctionChoice output_layer_activation = ActivationFunctionChoice.Sigmoid)
        {
            loss = LossFunction.Create();
            Hidden = new Layer(n_inputs, n_hidden_layer_neurons, hidden_layer_activation);
            Output = new Layer(n_hidden_layer_neurons, n_outputs, output_layer_activation);
        }

        /// <summary>Train the model with a single set of inputs and expected output.
        /// Returns the total loss prior to back propogation.</summary>
        public double Train(double[] inputs, double[] expected_outputs)
        {
            // Calculation
            var out_o1 = Hidden.Predict(inputs);
            var out_o2 = Output.Predict(out_o1);
            // Back propogation (stochastic gradient descent)
            var dloss_douto2 = loss.Derivative(out_o2, expected_outputs);
            Output.Train(out_o1, out_o2, dloss_douto2, LearningRate, out var dloss_dino2);
            // Next layer
            var dloss_douto1 = new double[Hidden.Neurons.Length];
            for (var n = 0; n < Output.Neurons.Length; n++)
            {
                var dloss_dino2_n = dloss_dino2[n];
                var neuron = Output.Neurons[n];
                for (var h = 0; h < Hidden.Neurons.Length; h++)
                    dloss_douto1[h] += dloss_dino2_n * neuron.Weights[h];
            }
            Hidden.Train(inputs, out_o1, dloss_douto1, learningRate);
            return loss.Calculate(dloss_douto2, out_o2, expected_outputs);
        }

        public double[] Predict(double[] inputs)
        {
            var result = Hidden.Predict(inputs);
            result = Output.Predict(result);
            return result;
        }

        public override string ToString() => $"Hidden: {string.Join(" / ", Hidden)}; Output: {string.Join(" / ", Output)};";
    }
}
