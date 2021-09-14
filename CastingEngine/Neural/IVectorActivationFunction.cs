using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    /// <summary>
    /// A derivable activation function which operates on a vector of weighted sums
    /// </summary>
    public interface IVectorActivationFunction
    {
        /// <summary>Calculates a vector of out_o from a vector of in_o</summary>
        public double[] Calculate(double[] weighted_sums);

        /// <summary>Calculates a vector of douto_dino from a vector of out_o</summary>
        public double[] Derivative(double[] outputs);
    }

    /// <summary>
    /// A derivable activation function which operates on a single weighted sum
    /// </summary>
    public interface IScalarActivationFunction
    {
        /// <summary>Calculates out_o from in_o</summary>
        public double Calculate(double weighted_sum);

        /// <summary>Calculates douto_dino from out_o</summary>
        public double Derivative(double output);
    }

    public abstract class ScalarActivationFunction : IScalarActivationFunction, IVectorActivationFunction
    {
        public double[] Calculate(double[] weighted_sums)
        {
            var result = new double[weighted_sums.Length];
            for (var i = 0; i < result.Length; i++)
                result[i] = Calculate(weighted_sums[i]);
            return result;
        }

        public double[] Derivative(double[] outputs)
        {
            var result = new double[outputs.Length];
            for (var i = 0; i < result.Length; i++)
                result[i] = Derivative(outputs[i]);
            return result;
        }

        public abstract double Calculate(double input);
        public abstract double Derivative(double input);
    }

    public class Sigmoid : ScalarActivationFunction
    {
        public override double Calculate(double input)
            => 1 / (1 + Math.Exp(-input));
        public override double Derivative(double input)
            => Calculate(input) * (1 - Calculate(input)); //TODO shortcut recalculation
    }

    public class Tanh : ScalarActivationFunction
    {
        public override double Calculate(double input)
            => Math.Tanh(input);

        public override double Derivative(double input)
            => 1 - Math.Pow(Calculate(input), 2);//TODO better way to do this?
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
        /// <summary>1/n * sum( (y-y0)^2 )</summary>
        public double Calculate(double[] outputs, double[] expected_outputs)
        {
            if (outputs.Length != expected_outputs.Length)
                throw new ArgumentException($"{nameof(outputs)} [{outputs.Length}] must have the same length as {nameof(expected_outputs)} [{expected_outputs.Length}]");
            if (outputs.Length == 0)
                throw new ArgumentException($"Cannot calculate loss for 0 outputs");
            double result = 0;
            for (var i = 0; i < outputs.Length; i++)
                result += Math.Pow(outputs[i] - expected_outputs[i], 2);
            return result / outputs.Length;
        }

        /// <summary>2/n * sum( y-y0 )</summary>
        public double Derivative(double[] outputs, double[] expected_outputs)
        {
            if (outputs.Length != expected_outputs.Length)
                throw new ArgumentException($"{nameof(outputs)} [{outputs.Length}] must have the same length as {nameof(expected_outputs)} [{expected_outputs.Length}]");
            if (outputs.Length == 0)
                throw new ArgumentException($"Cannot calculate loss derivative for 0 outputs");
            var result = 0.0;
            for (var i = 0; i < outputs.Length; i++)
                result += outputs[i] - expected_outputs[i];
            return 2 * result / outputs.Length;
        }
    }
}
