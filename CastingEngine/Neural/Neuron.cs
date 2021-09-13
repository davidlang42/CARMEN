using System;
using System.Linq;

namespace Carmen.CastingEngine.Neural
{
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

        public override string ToString() => $"[{string.Join(", ", Inputs.Select(i => i.Weight))}] + {Bias}";
    }
}
