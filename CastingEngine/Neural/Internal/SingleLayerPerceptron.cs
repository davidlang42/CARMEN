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
        public Layer Layer { get; init; }
        IEnumerable<Layer> INeuralNetwork.Layers => Layer.Yield();
        public int InputCount => Layer.Neurons.First().InputCount;
        public int OutputCount => Layer.NeuronCount;
        public double LearningRate { get; set; } = 0.05;

        private LossFunctionChoice lossFunction = LossFunctionChoice.MeanSquaredError;
        public LossFunctionChoice LossFunction
        {
            get => lossFunction;
            set
            {
                if (lossFunction == value)
                    return;
                lossFunction = value;
                loss = null;
            }
        }

        private ILossFunction? loss = null;
        private ILossFunction Loss => loss ??= LossFunction.Create();

        public SingleLayerPerceptron(int n_inputs, int n_outputs, ActivationFunctionChoice activation = ActivationFunctionChoice.Sigmoid)
        {
            Layer = new Layer(n_inputs, n_outputs, activation);
        }

        /// <summary>Train the model with a single set of inputs and expected outputs.
        /// Returns the total loss prior to back propogation.</summary>
        public double Train(double[] inputs, double[] expected_outputs)
        {
            // Calculation
            var out_o = Predict(inputs);
            // Back propogation (stochastic gradient descent)
            var dloss_douto = Loss.Derivative(out_o, expected_outputs);
            Layer.Train(inputs, out_o, dloss_douto, LearningRate, out _);
            return Loss.Calculate(dloss_douto, out_o, expected_outputs);
        }

        public double[] Predict(double[] inputs) => Layer.Predict(inputs);

        public override string ToString() => $"Outputs: {Layer}";
    }
}