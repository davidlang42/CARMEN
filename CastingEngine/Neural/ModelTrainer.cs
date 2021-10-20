using System;
using System.Collections.Generic;
using System.Linq;

namespace Carmen.CastingEngine.Neural
{
    public class ModelTrainer
    {
        INeuralNetwork model;

        /// <summary>The maximum number of training iterations per data point</summary>
        public int? MaxIterations { get; set; } = 10000;

        /// <summary>The threshold of loss at which any lower value is considered success</summary>
        public double? LossThreshold { get; set; } = 0.05;

        /// <summary>The threshold of change in loss at which any lower value is considered a failure to change</summary>
        public double? ChangeThreshold { get; set; } = 1e-10;

        public ModelTrainer(INeuralNetwork model)
        {
            this.model = model;
        }

        /// <summary>Train the model with a set of inputs and outputs, using stochastic gradient decent</summary>
        public Montage Train(IEnumerable<double[]> inputs, IEnumerable<double[]> expected_outputs)
        {
            if (MaxIterations == null && LossThreshold == null && ChangeThreshold == null)
                throw new ApplicationException($"At least one of ({nameof(MaxIterations)}, {nameof(LossThreshold)}, {nameof(ChangeThreshold)}) must be set.");
            if (MaxIterations.HasValue && MaxIterations.Value < 1)
                throw new ApplicationException($"{nameof(MaxIterations)} must be greater than 0.");
            var training_inputs = inputs.ToArray();
            var training_outputs = expected_outputs.ToArray();
            if (training_inputs.Length != training_outputs.Length)
                throw new ArgumentException($"Length of {nameof(inputs)}[{training_inputs.Length}] must equal the length of {nameof(expected_outputs)}[{training_outputs.Length}].");
            if (training_inputs.Length == 0)
                throw new ArgumentException($"{nameof(inputs)} cannot be empty");
            var repeat = 0;
            double[]? initial_loss = null;
            var previous_loss = new double[training_inputs.Length];
            var success = false;
            var contains_nan = ContainsNaN(model);
            var no_change = false;
            var too_many_repeats = false;
            var descriptions = new List<string>();
            var change_threshold = ChangeThreshold;
            while (!success && !no_change && !too_many_repeats && !contains_nan)
            {
                descriptions.Add(model.ToString());
                no_change = true;
                for (var i = 0; i < training_inputs.Length; i++)
                {
                    var new_loss = model.Train(training_inputs[i], training_outputs[i]);
                    if (no_change && change_threshold.HasValue)
                        no_change = Math.Abs(new_loss - previous_loss[i]) < change_threshold.Value; // possibly use average loss in the future
                    previous_loss[i] = new_loss;
                }
                if (LossThreshold is double loss_threshold)
                    success = previous_loss.All(loss => loss < loss_threshold);
                contains_nan = ContainsNaN(model);
                initial_loss ??= previous_loss.ToArray();
                too_many_repeats = ++repeat >= MaxIterations && MaxIterations.HasValue;
            }
            return new Montage
            {
                InitialLoss = initial_loss ?? new double[0],
                Success = success,
                ReachedMaxIterations = too_many_repeats,
                ReachedStableLoss = no_change,
                ContainsNaN = contains_nan,
                Iterations = repeat,
                FinalLoss = previous_loss,
                Descriptions = descriptions
            };
        }

        private static bool ContainsNaN(INeuralNetwork model)
            => model.Layers.Any(l => l.Neurons.Any(n => double.IsNaN(n.Bias) || n.Weights.Any(w => double.IsNaN(w))));
    }
}
