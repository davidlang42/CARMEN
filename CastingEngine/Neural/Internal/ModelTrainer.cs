using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural.Internal
{
    /// <summary>
    /// An artificial neural network which takes input features from an object of type <typeparamref name="T"/>
    /// and predicts output features as an object of type <typeparamref name="U"/>.
    /// </summary>
    //LATER remove DeprecatedNeuralNetwork<T, U>
    public class DeprecatedNeuralNetwork<T, U>
        where T : struct, IInputFeatureSet
        where U: struct, IOutputFeatureSet
    {
        FeedforwardNetwork network;

        /// <summary>The maximum number of training iterations per data point</summary>
        public int? MaxIterations { get; set; } = 10000;

        /// <summary>The threshold of loss at which any lower value is considered success</summary>
        public double? LossThreshold { get; set; } = 0.05;

        /// <summary>The threshold of change in loss at which any lower value is considered a failure to change</summary>
        public double? ChangeThreshold { get; set; } = 1e-10;

        /// <summary>The speed at which the neural network learns, smaller is safer</summary>
        public double LearningRate
        {
            get => network.LearningRate;
            set => network.LearningRate = value;
        }

        public DeprecatedNeuralNetwork(int n_hidden_layers, int n_neurons_per_hidden_layer,
            ActivationFunctionChoice hidden_layer_activation = ActivationFunctionChoice.Tanh,
            ActivationFunctionChoice output_layer_activation = ActivationFunctionChoice.Sigmoid)
        {
            network = new FeedforwardNetwork(IInputFeatureSet.GetSize<T>(), n_hidden_layers, n_neurons_per_hidden_layer,
                IInputFeatureSet.GetSize<U>(), hidden_layer_activation, output_layer_activation);
        }

        /// <summary>Train the model with a set of inputs and outputs, using stochastic gradient decent</summary>
        public Montage Train(IEnumerable<T> inputs, IEnumerable<U> expected_outputs)
        {
            if (MaxIterations == null && LossThreshold == null && ChangeThreshold == null)
                throw new ApplicationException($"At least one of ({nameof(MaxIterations)}, {nameof(LossThreshold)}, {nameof(ChangeThreshold)}) must be set.");
            var training_inputs = inputs.Select(i => i.GetValues()).ToArray();
            var training_outputs = expected_outputs.Select(o => o.GetValues()).ToArray();
            if (training_inputs.Length != training_outputs.Length)
                throw new ArgumentException($"Length of {nameof(inputs)}[{training_inputs.Length}] must equal the length of {nameof(expected_outputs)}[{training_outputs.Length}].");
            var repeat = 0;
            var previous_loss = new double[training_inputs.Length];
            var success = false;
            var no_change = false;
            var too_many_repeats = false;
            var descriptions = new List<string>();
            while(!success && !no_change && !too_many_repeats) //LATER maybe reduce the learning rate when no change detected
            {
                descriptions.Add(network.ToString());
                success = true;
                no_change = true;
                for (var i = 0; i < training_inputs.Length; i++)
                {
                    var new_loss = network.Train(training_inputs[i], training_outputs[i]);
                    no_change &= ChangeThreshold.HasValue && Math.Abs(new_loss - previous_loss[i]) < ChangeThreshold;
                    previous_loss[i] = new_loss;
                    success &= LossThreshold.HasValue && new_loss < LossThreshold;
                }
                too_many_repeats = ++repeat >= MaxIterations && MaxIterations.HasValue;
            }
            return new Montage
            {
                Success = success,
                ReachedMaxIterations = too_many_repeats,
                ReachedStableLoss = no_change,
                Iterations = repeat,
                FinalLoss = previous_loss,
                Descriptions = descriptions
            };
        }

        public U Predict(T input)
        {
            var result = new U();
            result.SetValues(network.Predict(input.GetValues()));
            return result;
        }

        public override string ToString() => network.ToString();
    }

    public struct Montage
    {
        public bool Success;
        public bool ReachedMaxIterations;
        public bool ReachedStableLoss;
        public bool ContainsNaN;
        public double[] InitialLoss;
        public double[] FinalLoss;
        public int Iterations;
        public List<string> Descriptions;

        public double InitialAverageLoss => InitialLoss.Average();
        public double FinalAverageLoss => FinalLoss.Average();
    }

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
            var repeat = 0;
            double[]? initial_loss = null;
            var previous_loss = new double[training_inputs.Length];
            var success = false;
            var contains_nan = ContainsNaN(model);
            var no_change = false;
            var too_many_repeats = false;
            var descriptions = new List<string>();
            while (!success && !no_change && !too_many_repeats && !contains_nan)
            {
                descriptions.Add(model.ToString());
                success = true;
                no_change = true;
                for (var i = 0; i < training_inputs.Length; i++)
                {
                    var new_loss = model.Train(training_inputs[i], training_outputs[i]);
                    no_change &= ChangeThreshold.HasValue && Math.Abs(new_loss - previous_loss[i]) < ChangeThreshold; //LATER is this logic even right? shouldn't it be average loss?
                    previous_loss[i] = new_loss;
                    success &= LossThreshold.HasValue && new_loss < LossThreshold;
                }
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
