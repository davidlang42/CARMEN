﻿using System;
using System.Linq;

namespace Carmen.CastingEngine.Neural
{
    /// <summary>
    /// Represents a single neuron in an artificial neural network.
    /// Mathematically, this calculates the weighted sum of its inputs, but does not include the activation function.
    /// </summary>
    public class Neuron
    {
        public const double MINIMUM_SEED = 0.2;
        public const double MAXIMUM_SEED = 0.8;

        readonly public  double[] Weights; // only the array size is readonly
        public double Bias;

        public int InputCount => Weights.Length;

        /// <summary>Parameterless constructor for serialisation</summary>
        private Neuron()
            => Weights = Array.Empty<double>();

        /// <summary>Create a neuron with random weights and bias</summary>
        public Neuron(int n_inputs, Random? random = null)
        {
            random ??= new Random();
            Weights = new double[n_inputs];
            for (var i = 0; i < Weights.Length; i++)
                Weights[i] = random.NextDouble() * (MAXIMUM_SEED - MINIMUM_SEED) + MINIMUM_SEED;
            Bias = random.NextDouble() * (MAXIMUM_SEED - MINIMUM_SEED) + MINIMUM_SEED;
        }

        /// <summary>Load a neuron with existings weights and bias</summary>
        public Neuron(double[] weights, double bias)
        {
            Weights = weights;
            Bias = bias;
        }

        /// <summary>Calculates the weighted sum (with bias) of the inputs</summary>
        public double WeightedSum(double[] inputs)
        {
            var result = Bias;
            if (Weights.Length != inputs.Length)
                throw new ArgumentException($"{nameof(inputs)}[{inputs.Length}] must have the same length as {nameof(Weights)}[{Weights.Length}]");
            for (var i = 0; i < inputs.Length; i++)
                result += Weights[i] * inputs[i];
            return result;
        }

        public override string ToString() => $"[{string.Join(", ", Weights.Select(w => $"{w:0.000}"))}] + {Bias:0.000}";
    }
}
