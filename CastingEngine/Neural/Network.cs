﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    public class SingleNeuronNetwork
    {
        IActivationFunction activation = new Sigmoid();
        IErrorFunction error = new MeanSquaredError();
        public Neuron Neuron = new Neuron
        {
            Inputs = new[]
            {
                new InputFeature { Weight = 0.1 },
                new InputFeature { Weight = 0.2 }
            },
            Bias = 0.3
        };
        double learningRate = 0.05;

        public void Train(double[] inputs, double expected_output)
        {
            // Calculation
            var out_o = Predict(inputs);
            // Back propogation (gradient descent)
            //TODO var mse = error.Calculate(out_o, expected_output);
            var derror_douto = error.Derivative(out_o, expected_output);
            var douto_dino = activation.Derivative(out_o);
            var derror_dino = derror_douto * douto_dino;
            for (var i = 0; i < inputs.Length; i++)
            {
                var dino_dweight = inputs[i]; // because weighted sum: dino = i0*w0 + i1*w1 + bias
                var derror_dweight = derror_dino * dino_dweight;
                Neuron.Inputs[i].Weight -= learningRate * derror_dweight;
            }
            var dino_dbias = 1; // because weighted sum: dino = i0*w0 + i1*w1 + bias
            Neuron.Bias -= learningRate * derror_dino * dino_dbias;
        }

        public double Predict(double[] inputs)
        {
            var in_o = Neuron.Calculate(inputs);
            var out_o = activation.Calculate(in_o);
            return out_o;
        }
    }

    public struct Neuron
    {
        public InputFeature[] Inputs { get; set; }
        public double Bias { get; set; }

        public double Calculate(double[] values)
        {
            var result = Bias;
            if (Inputs.Length != values.Length)
                throw new ArgumentException($"{nameof(values)} [{values.Length}] must have the same length as {nameof(Inputs)} [{Inputs.Length}]");
            for (var i = 0; i < values.Length; i++)
                result += Inputs[i].Calculate(values[i]);
            return result;
        }
    }

    public class InputFeature
    {
        public double Calculate(double input) => input * Weight;
        public double Weight { get; set; }
    }
}
