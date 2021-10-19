using System.Collections.Generic;
using System.Linq;

namespace Carmen.CastingEngine.Neural
{
    /// <summary>
    /// A neural network consisting of a single layer of neurons from inputs to outputs.
    /// This is a very simple type of network, which is only capable of predicting data which is linearly separable.
    /// </summary>
    public class SingleLayerPerceptron : INeuralNetwork
    {
        readonly public Layer Layer;
        IEnumerable<Layer> INeuralNetwork.Layers => Layer.Yield();
        public int InputCount => Layer.Neurons.First().InputCount;
        public int OutputCount => Layer.NeuronCount;
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
                loss = lossFunction.Create();
            }
        }

        private ILossFunction loss;

        public SingleLayerPerceptron(int n_inputs, int n_outputs, ActivationFunctionChoice activation = ActivationFunctionChoice.Sigmoid)
        {
            loss = lossFunction.Create();
            Layer = new Layer(n_inputs, n_outputs, activation);
        }

        /// <summary>Train the model with a single set of inputs and expected outputs.
        /// Returns the total loss prior to back propogation.</summary>
        public double Train(double[] inputs, double[] expected_outputs)
        {
            // Calculation
            var out_o = Predict(inputs);
            // Back propogation (stochastic gradient descent)
            var dloss_douto = loss.Derivative(out_o, expected_outputs);
            Layer.Train(inputs, out_o, dloss_douto, learningRate);
            return loss.Calculate(dloss_douto, out_o, expected_outputs);
        }

        public double[] Predict(double[] inputs) => Layer.Predict(inputs);

        public override string ToString() => $"Outputs: {Layer}";
    }
}