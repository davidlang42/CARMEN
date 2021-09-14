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

        /// <summary>Calculates out_o from in_o</summary>
        public abstract double Calculate(double weighted_sum);

        /// <summary>Calculates douto_dino from out_o</summary>
        public abstract double Derivative(double output);
    }

    public class Sigmoid : ScalarActivationFunction
    {
        /// <summary>Calculates out_o from in_o</summary>
        public override double Calculate(double weighted_sum)
            => 1 / (1 + Math.Exp(-weighted_sum));

        /// <summary>Calculates douto_dino from out_o</summary>
        public override double Derivative(double output)
            => output * (1 - output);
    }

    public class Tanh : ScalarActivationFunction
    {
        /// <summary>Calculates out_o from in_o</summary>
        public override double Calculate(double weighted_sum)
            => Math.Tanh(weighted_sum);

        /// <summary>Calculates douto_dino from out_o</summary>
        public override double Derivative(double output)
            => 1 - Math.Pow(output, 2); //LATER is x * x faster than Math.Pow(x, 2)?
    }

    public interface ILossFunction
    {
        /// <summary>Calculates the total error across all outputs</summary>
        public double Calculate(double[] outputs, double[] expected_outputs);

        /// <summary>Calculates the partial derivative of the error in respect to each output</summary>
        public double[] Derivative(double[] outputs, double[] expected_outputs);
    }

    public class MeanSquaredError : ILossFunction
    {
        /// <summary>1/2 * sum( (y-y')^2 )</summary>
        public double Calculate(double[] outputs, double[] expected_outputs) //LATER speed up by re-using previously calculated derivative passed in as an argument
        {
            if (outputs.Length != expected_outputs.Length)
                throw new ArgumentException($"{nameof(outputs)} [{outputs.Length}] must have the same length as {nameof(expected_outputs)} [{expected_outputs.Length}]");
            if (outputs.Length == 0)
                throw new ArgumentException($"Cannot calculate loss for 0 outputs");
            double result = 0;
            for (var i = 0; i < outputs.Length; i++)
                result += Math.Pow(expected_outputs[i] - outputs[i], 2); //LATER is x * x faster than Math.Pow(x, 2)?
            return result / 2;
        }

        /// <summary>y'-y</summary>
        public double[] Derivative(double[] outputs, double[] expected_outputs)
        {
            if (outputs.Length != expected_outputs.Length)
                throw new ArgumentException($"{nameof(outputs)} [{outputs.Length}] must have the same length as {nameof(expected_outputs)} [{expected_outputs.Length}]");
            if (outputs.Length == 0)
                throw new ArgumentException($"Cannot calculate loss derivative for 0 outputs");
            var result = new double[outputs.Length];
            for (var i = 0; i < outputs.Length; i++)
                result[i] = outputs[i] - expected_outputs[i];
            return result;
        }
    }
}
