using System;
using System.Linq;

namespace Carmen.CastingEngine.Neural
{
    public class Neuron
    {
        public InputFeature[] Inputs { get; set; }
        public double Bias { get; set; }

        public Neuron(int n_inputs)
        {
            Inputs = new InputFeature[n_inputs];
            for (var i = 0; i < Inputs.Length; i++)
                Inputs[i] = new InputFeature
                {
                    Weight = new Random().NextDouble()
                };
        }

        public double Calculate(double[] values)
        {
            var result = Bias;
            if (Inputs.Length != values.Length)
                throw new ArgumentException($"{nameof(values)} [{values.Length}] must have the same length as {nameof(Inputs)} [{Inputs.Length}]");
            for (var i = 0; i < values.Length; i++)
                result += Inputs[i].Calculate(values[i]);
            return result;
        }

        public override string ToString() => $"[{string.Join(", ", Inputs.Select(i => i.Weight))}] + {Bias}";
    }
}
