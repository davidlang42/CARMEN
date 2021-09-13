using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    public class MultiLayerPerceptron
    {
        ILossFunction loss = new MeanSquaredError();
        Layer hidden, output;
        double learningRate = 0.05;

        public MultiLayerPerceptron(int n_inputs, int n_hidden_layer_neurons, int n_outputs)
        {
            hidden = new Layer(n_inputs, n_hidden_layer_neurons);
            output = new Layer(n_hidden_layer_neurons, n_outputs);
        }

        public void Train(double[] inputs, double[] expected_outputs)
        {
            //// Calculation
            //var out_o = Predict(inputs);
            //// Back propogation (stochastic gradient descent)
            //var dloss_douto = new double[Neurons.Length];
            //var douto_dino = new double[Neurons.Length];
            //var dloss_dino = new double[Neurons.Length];
            //for (var l = 0; l < Layers.Length; l++)
            //{


            //    dloss_douto[n] = loss.Derivative(out_o[n], expected_outputs[n]);
            //    douto_dino[n] = activation.Derivative(out_o[n]);
            //    dloss_dino[n] = dloss_douto[n] * douto_dino[n];
            //    for (var i = 0; i < inputs.Length; i++)
            //    {
            //        var dino_dweight = inputs[i]; // because weighted sum: dino = i0*w0 + i1*w1 + bias
            //        var dloss_dweight = dloss_dino[n] * dino_dweight;
            //        Neurons[n].Inputs[i].Weight -= learningRate * dloss_dweight;
            //    }
            //    var dino_dbias = 1; // because weighted sum: dino = i0*w0 + i1*w1 + bias
            //    Neurons[n].Bias -= learningRate * dloss_dino[n] * dino_dbias;
            //}
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
