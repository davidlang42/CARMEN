﻿using System;
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
            // Calculation
            var out_o = Predict(inputs);
            // Back propogation (stochastic gradient descent)
            var dloss_douto = new double[output.Neurons.Length];
            for (var n = 0; n < output.Neurons.Length; n++)
                dloss_douto[n] = loss.Derivative(out_o[n], expected_outputs[n]);//TODO make this a single array call
            output.Train(inputs, out_o, dloss_douto, learningRate);
            // Next layer
            out_o = hidden.Predict(inputs); //TODO re-calculating
            var dloss_douto_hidden = new double[hidden.Neurons.Length];
            for (var h = 0; h < hidden.Neurons.Length; h++)
                for (var n = 0; n < output.Neurons.Length; n++)
                    dloss_douto_hidden[h] += dloss_douto[n] * output.Neurons[n].Inputs[h].Weight;
            hidden.Train(inputs, out_o, dloss_douto_hidden, learningRate);
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
