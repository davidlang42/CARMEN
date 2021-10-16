using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural.Internal
{
    public class Layer
    {
        private ActivationFunctionChoice activationFunction = ActivationFunctionChoice.Sigmoid;
        public ActivationFunctionChoice ActivationFunction
        {
            get => activationFunction;
            set
            {
                if (activationFunction == value)
                    return;
                activationFunction = value;
                activation = activationFunction.Create();
            }
        }

        private IVectorActivationFunction activation;

        readonly public Neuron[] Neurons; // only the array size is readonly

        public int NeuronCount => Neurons.Length;

        /// <summary>Parameterless constructor for serialisation</summary>
        private Layer()
        {
            activation = activationFunction.Create();
            Neurons = Array.Empty<Neuron>();
        }

        /// <summary>Create a layer of neurons, each with random weights and biases,
        /// utilising a common activation function</summary>
        public Layer(int n_inputs, int n_outputs, ActivationFunctionChoice activation_function = ActivationFunctionChoice.Sigmoid, Random? random = null)
        {
            activationFunction = activation_function;
            activation = activationFunction.Create();
            Neurons = new Neuron[n_outputs];
            for (var n = 0; n < Neurons.Length; n++)
                Neurons[n] = new Neuron(n_inputs, random);
        }

        /// <summary>Load a layer of neurons, each with existings weights and biases,
        /// utilising a common activation function</summary>
        public Layer(double[][] neuron_weights, double[] neuron_biases, ActivationFunctionChoice activation_function = ActivationFunctionChoice.Sigmoid)
        {
            if (neuron_weights.Length != neuron_biases.Length)
                throw new ArgumentException($"{nameof(neuron_weights)}[{neuron_weights.Length}] must have the same length as {nameof(neuron_biases)}[{neuron_biases.Length}]");
            activationFunction = activation_function;
            activation = activationFunction.Create();
            Neurons = new Neuron[neuron_weights.Length];
            for (var n = 0; n < Neurons.Length; n++)
                Neurons[n] = new Neuron(neuron_weights[n], neuron_biases[n]);
        }

        /// <summary>Train this layer of the model</summary>
        public void Train(double[] inputs, double[] out_o, double[] dloss_douto, double learningRate, out double[] dloss_dino)
        {
            var douto_dino = activation.Derivative(out_o);
            dloss_dino = new double[Neurons.Length];
            for (var n = 0; n < Neurons.Length; n++)
            {
                var neuron = Neurons[n];
                dloss_dino[n] = dloss_douto[n] * douto_dino[n];
                for (var i = 0; i < inputs.Length; i++)
                {
                    var dino_dweight = inputs[i]; // because weighted sum: dino = i0*w0 + i1*w1 + bias
                    var dloss_dweight = dloss_dino[n] * dino_dweight;
                    neuron.Weights[i] -= learningRate * dloss_dweight;
                }
                //TODO is it faster to make this 1 a constant rather than variable? or leave it out completely?
                var dino_dbias = 1; // because weighted sum: dino = i0*w0 + i1*w1 + bias
                neuron.Bias -= learningRate * dloss_dino[n] * dino_dbias;
            }
        }

        public double[] Predict(double[] inputs)
        {
            var in_o = new double[Neurons.Length];
            for (var i = 0; i < in_o.Length; i++)
                in_o[i] = Neurons[i].WeightedSum(inputs);
            return activation.Calculate(in_o);
        }

        public override string ToString() => string.Join(" / ", Neurons.Select(n => n.ToString()));
    }
}
