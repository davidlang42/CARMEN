using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    public class Layer
    {
        IActivationFunction activation = new Sigmoid();
        public Neuron[] Neurons;

        public Layer(int n_inputs, int n_outputs)
        {
            Neurons = new Neuron[n_outputs];
            for (var n = 0; n < Neurons.Length; n++)
                Neurons[n] = new Neuron(n_inputs);
        }

        public void Train(double[] inputs, double[] out_o, double[] dloss_douto, double learningRate)
        {
            var douto_dino = new double[Neurons.Length];
            var dloss_dino = new double[Neurons.Length];
            for (var n = 0; n < Neurons.Length; n++)
            {
                douto_dino[n] = activation.Derivative(out_o[n]);//TODO make this one array function call
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
            for (var i = 0; i < result.Length; i++)
            {
                var in_o = Neurons[i].Calculate(inputs);
                var out_o = activation.Calculate(in_o);
                result[i] = out_o;
            }
            return result;
        }

        public override string ToString() => string.Join(" / ", Neurons.Select(n => n.ToString()));
    }
}
