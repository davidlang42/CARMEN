using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural.Internal
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
            => 1 - output * output;
    }

    public class ReLu : ScalarActivationFunction
    {
        /// <summary>Calculates out_o from in_o</summary>
        public override double Calculate(double weighted_sum)
            => weighted_sum > 0 ? weighted_sum : 0;

        /// <summary>Calculates douto_dino from out_o</summary>
        public override double Derivative(double output)
            => output > 0 ? 1 : 0;
    }

    public class ParametricReLu : ScalarActivationFunction
    {
        private readonly double alpha;

        public ParametricReLu(double alpha)
        {
            this.alpha = alpha;
        }

        /// <summary>Calculates out_o from in_o</summary>
        public override double Calculate(double weighted_sum)
            => weighted_sum > 0 ? weighted_sum : weighted_sum * alpha;

        /// <summary>Calculates douto_dino from out_o</summary>
        public override double Derivative(double output)
            => output > 0 ? 1 : alpha;
    }

    public class LeakyReLu : ParametricReLu
    {
        public LeakyReLu() : base(0.01)
        { }
    }

    public class ExponentialLu : ScalarActivationFunction
    {
        /// <summary>Calculates out_o from in_o</summary>
        public override double Calculate(double weighted_sum)
            => weighted_sum > 0 ? weighted_sum : Math.Exp(weighted_sum) - 1;

        /// <summary>Calculates douto_dino from out_o</summary>
        public override double Derivative(double output)
            => output > 0 ? 1 : output + 1;
    }
}
