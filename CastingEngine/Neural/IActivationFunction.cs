using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    public interface IActivationFunction
    {
        public double Calculate(double input);
        public double Derivative(double input);
    }

    public class Sigmoid : IActivationFunction
    {
        public double Calculate(double input)
            => 1 / (1 + Math.Exp(-input));
        public double Derivative(double input)
            => Calculate(input) * (1 - Calculate(input)); //TODO shortcut recalculation
    }

    public interface ILossFunction
    {
        public double Calculate(double[] outputs, double[] expected_outputs);
        public double Derivative(double[] outputs, double[] expected_outputs);

        public double Calculate(double output, double expected_output)
            => Calculate(new[] { output }, new[] { expected_output });
        public double Derivative(double output, double expected_output)
            => Derivative(new[] { output }, new[] { expected_output });
    }

    public class MeanSquaredError : ILossFunction
    {
        public double Calculate(double[] outputs, double[] expected_outputs)
        {
            throw new Exception("Currently the derivative doesn't match this function!?");
            if (outputs.Length != expected_outputs.Length)
                throw new ArgumentException($"{nameof(outputs)} [{outputs.Length}] must have the same length as {nameof(expected_outputs)} [{expected_outputs.Length}]");
            if (outputs.Length == 0)
                return 0;
            var result = 0.0;
            for (var i = 0; i < outputs.Length; i++)
                result += Math.Pow(outputs[i] - expected_outputs[i], 2);
            return result / outputs.Length;
        }

        public double Derivative(double[] outputs, double[] expected_outputs)
        {
            if (outputs.Length != expected_outputs.Length)
                throw new ArgumentException($"{nameof(outputs)} [{outputs.Length}] must have the same length as {nameof(expected_outputs)} [{expected_outputs.Length}]");
            if (outputs.Length == 0)
                return 0;
            var result = 0.0;
            for (var i = 0; i < outputs.Length; i++)
                result += outputs[i] - expected_outputs[i];
            return result / outputs.Length;
        }
    }
}
