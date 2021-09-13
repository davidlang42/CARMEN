using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    public class SingleLayerNetwork
    {
        IActivationFunction activation = new Sigmoid();
        ILossFunction loss = new MeanSquaredError();
        public Neuron[] Neurons = new[]
        {
            new Neuron
            {
                Inputs = new[]
                {
                    new InputFeature { Weight = 0.1 },
                    new InputFeature { Weight = 0.2 }
                },
                Bias = 0.3
            },
            new Neuron
            {
                Inputs = new[]
                {
                    new InputFeature { Weight = 0.1 },
                    new InputFeature { Weight = 0.2 }
                },
                Bias = 0.3
            }
        };
        double learningRate = 0.05;

        public void Train(double[] inputs, double[] expected_outputs)
        {
            // Calculation
            var out_o = Predict(inputs);
            // Back propogation (stochastic gradient descent)
            var dloss_douto = new double[Neurons.Length];
            var douto_dino = new double[Neurons.Length];
            var dloss_dino = new double[Neurons.Length];
            for (var n = 0; n < Neurons.Length; n++)
            {
                dloss_douto[n] = loss.Derivative(out_o[n], expected_outputs[n]);
                douto_dino[n] = activation.Derivative(out_o[n]);
                dloss_dino[n] = dloss_douto[n] * douto_dino[n];
                for (var i = 0; i < inputs.Length; i++)
                {
                    var dino_dweight = inputs[i]; // because weighted sum: dino = i0*w0 + i1*w1 + bias
                    var dloss_dweight = dloss_dino[n] * dino_dweight;
                    Neurons[n].Inputs[i].Weight -= learningRate * dloss_dweight;
                }
                var dino_dbias = 1; // because weighted sum: dino = i0*w0 + i1*w1 + bias
                Neurons[n].Bias -= learningRate * dloss_dino[n] * dino_dbias;
            }
        }

        public double[] Predict(double[] inputs)
        {
            var result = new double[Neurons.Length];
            for (var i=0; i< result.Length; i++)
            {
                var in_o = Neurons[i].Calculate(inputs);
                var out_o = activation.Calculate(in_o);
                result[i] = out_o;
            }
            return result;
        }

        public override string ToString() => $"Outputs: {string.Join(" / ", Neurons)}";
    }
}