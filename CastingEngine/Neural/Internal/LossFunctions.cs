using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural.Internal
{
    /// <summary>
    /// A derivable loss function which operates on vectors of outputs and expected outputs
    /// </summary>
    public interface ILossFunction
    {
        /// <summary>Calculates the total error across all outputs using the
        /// already calculated partial derivatives</summary>
        public double Calculate(double[] dloss_douto);

        /// <summary>Calculates the partial derivative of the error with respect
        /// to each output (ie. dloss_douto)</summary>
        public double[] Derivative(double[] outputs, double[] expected_outputs);
    }

    public class MeanSquaredError : ILossFunction
    {
        /// <summary>1/2 * sum( (y-y')^2 )</summary>
        public double Calculate(double[] dloss_douto)
        {
            double result = 0;
            for (var i = 0; i < dloss_douto.Length; i++)
                result += Math.Pow(dloss_douto[i], 2); //LATER is x * x faster than Math.Pow(x, 2)?
            return result / 2; //LATER I still feel like this should be average loss, therefore divide by dloss_douto.Length, this may not have mattered because I was almost always predicting 1 output
        }

        /// <summary>y'-y</summary>
        public double[] Derivative(double[] outputs, double[] expected_outputs)
        {
            if (outputs.Length != expected_outputs.Length)
                throw new ArgumentException($"{nameof(outputs)} [{outputs.Length}] must have the same length as {nameof(expected_outputs)} [{expected_outputs.Length}]");
            var result = new double[outputs.Length];
            for (var i = 0; i < outputs.Length; i++)
                result[i] = outputs[i] - expected_outputs[i];
            return result;
        }
    }

    public class ClassificationError : ILossFunction
    {
        /// <summary>The threshold for correctness.
        /// A value greater than 0.5 allows errors to be counted as correct.</summary>
        public double Threshold { get; set; } = 0.5;

        /// <summary>count(incorrect prediction)</summary>
        public double Calculate(double[] dloss_douto)
        {
            // output: [0, 0.5], expected: 0, dloss_douto: [0, 0.5], result: correct
            // output: [0.5, 1], expected: 0, dloss_douto: [0.5, 1], result: incorrect
            // output: [0, 0.5], expected: 1, dloss_douto: [-1, -0.5], result: incorrect
            // output: [0.5, 1], expected: 1, dloss_douto: [-0.5, 0], result: correct
            int incorrect = 0;
            for (var i = 0; i < dloss_douto.Length; i++)
                if (Math.Abs(dloss_douto[i]) > Threshold)
                    incorrect++;
            return incorrect;
        }

        /// <summary>y'-y</summary>
        public double[] Derivative(double[] outputs, double[] expected_outputs)
        {
            if (outputs.Length != expected_outputs.Length)
                throw new ArgumentException($"{nameof(outputs)} [{outputs.Length}] must have the same length as {nameof(expected_outputs)} [{expected_outputs.Length}]");
            var result = new double[outputs.Length];
            for (var i = 0; i < outputs.Length; i++)
                result[i] = outputs[i] - expected_outputs[i];
            return result;
        }
    }
}
