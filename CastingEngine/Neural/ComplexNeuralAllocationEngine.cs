using Carmen.CastingEngine.Base;
using Carmen.CastingEngine.Heuristic;
using Carmen.CastingEngine.Neural.Internal;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Carmen.CastingEngine.Neural
{
    /// <summary>
    /// A concrete approach for learning the user's casting choices, using a Feed-forward Neural Network
    /// with customisable complexity.
    /// NOTE: This complexity requires storage of the neural network weights outside the ShowModel
    /// </summary>
    public class ComplexNeuralAllocationEngine : AllocationEngine, IComparer<(Applicant, Role)> //TODO update summary comment of NeuralAllocationEngine if not extending it
    {
        readonly IOverallWeighting[] overallWeightings;
        readonly Requirement[] suitabilityRequirements;
        readonly ICriteriaRequirement[] existingRoleRequirements;
        readonly FeedforwardNetwork model;
        readonly int nInputs;
        readonly UserConfirmation confirm;
        readonly Dictionary<double[], double[]> trainingPairs = new();

        //LATER allow users to change these parameters
        #region Engine parameters
        /// <summary>The maximum number of training iterations run per invocation of
        /// <see cref="UserPickedCast(IEnumerable{Applicant}, IEnumerable{Applicant}, Role)"/></summary>
        public int MaxTrainingIterations { get; set; } = 100;

        /// <summary>The speed at which the neural network learns from results, as a fraction of the sum of
        /// <see cref="Requirement.SuitabilityWeight"/>. Reasonable values are between 0.001 and 0.01.
        /// WARNING: Changing this can have crazy consequences, slower is generally safer but be careful.</summary>
        public double NeuralLearningRate { get; set; } = 0.005;

        /// <summary>Determines which loss function is used when training the neural network.</summary>
        public LossFunctionChoice NeuralLossFunction { get; set; } = LossFunctionChoice.Classification0_4;

        /// <summary>If true, training will occur whenever <see cref="UserPickedCast(IEnumerable{Applicant}, IEnumerable{Applicant}, Role)"/> is called</summary>
        public bool TrainImmediately { get; set; } = false;

        /// <summary>If true, training data will kept to be used again in future training</summary>
        public bool StockpileTrainingData { get; set; } = true;

        /// <summary>If false, the model will only be used for predictions, but not updated</summary>
        public bool AllowTraining { get; set; } = true;

        public string ModelFileName { get; private init; } //LATER make this a setting, editable by user
        #endregion

        public ComplexNeuralAllocationEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, ShowRoot show_root, Requirement[] requirements, UserConfirmation confirm, string model_file_name)
            : base(applicant_engine, alternative_casts)
        {
            // Find out what (and how many) things will have overall ability weights
            overallWeightings = show_root.Yield().ToArray();
            // Find the requirements which will be used for role suitability
            suitabilityRequirements = requirements;
            // Find the requirements which will detract based on existing roles
            existingRoleRequirements = requirements.OfType<ICriteriaRequirement>().ToArray();
            // Construct the model
            this.confirm = confirm;
            nInputs = (overallWeightings.Length + suitabilityRequirements.Length + existingRoleRequirements.Length) * 2;
            ModelFileName = model_file_name;
            model = LoadModelFromDisk(nInputs, model_file_name);
            //TODO allow parameters to be configured (layers, neurons, hidden activation functions)
        }

        private FeedforwardNetwork LoadModelFromDisk(int n_inputs, string file_name)
        {
            if (!string.IsNullOrEmpty(file_name) && File.Exists(file_name))
            {
                var reader = new XmlSerializer(typeof(FeedforwardNetwork));
                try
                {
                    using var file = new StreamReader(file_name);
                    if (reader.Deserialize(file) is FeedforwardNetwork model && model.InputCount == n_inputs)
                        return model;
                }
                catch
                {
                    //LATER log exception or otherwise tell user, there are many cases that can get here: file access issue, corrupt/invalid file format, file contains model with wrong number of inputs
                }
            }
            var new_model = new FeedforwardNetwork(n_inputs, 2, n_inputs, 1); // sigmoid output is between 0 and 1, crossing at 0.5
            foreach (var neuron in new_model.Layers.First().Neurons)
                FlipPolarities(neuron);
            return new_model;
        }

        private static void SaveModelToDisk(string file_name, FeedforwardNetwork model)
        {
            var writer = new XmlSerializer(typeof(FeedforwardNetwork));
            //LATER handle exceptions
            using var file = new StreamWriter(file_name);
            writer.Serialize(file, model);
        }

        #region Business logic
        public override bool UserPickedCast(IEnumerable<Applicant> applicants_picked, IEnumerable<Applicant> applicants_not_picked, Role role)
        {
            if (!AllowTraining)
                return false; // nothing to do
            if (role.Requirements.Count(r => suitabilityRequirements.Contains(r)) == 0)//TODO think about this
                return false; // nothing to do
            // Generate training data
            var not_picked_array = applicants_not_picked.ToArray();
            if (not_picked_array.Length == 0)
                return false; // nothing to do
            var training_pairs = new Dictionary<double[], double[]>();
            foreach (var (picked, not_picked) in NeuralApplicantEngine.ComparablePairs(applicants_picked, not_picked_array))
            {
                training_pairs.Add(InputValues(picked, not_picked, role), new[] { 1.0 });
                training_pairs.Add(InputValues(not_picked, picked, role), new[] { 0.0 });
            }
            if (!training_pairs.Any())
                return false; // nothing to do
            TrainingPairsAdded(training_pairs, role);
            return false;
        }

        public override bool ExportChanges()
        {
            if (AllowTraining)
            {
                FinaliseTraining();
                SaveModelToDisk(ModelFileName, model);
            }
            return false;
        }

        protected IEnumerable<IWeightChange> TrainingPairsAdded(Dictionary<double[], double[]> pairs, Role role)
        {
            foreach (var pair in pairs)
                trainingPairs.Add(pair.Key, pair.Value);
            if (!TrainImmediately)
                return Enumerable.Empty<IWeightChange>(); // do it later
            return FinaliseTraining();
        }

        protected IEnumerable<IWeightChange> FinaliseTraining()
        {
            if (!trainingPairs.Any())
                return Enumerable.Empty<IWeightChange>(); // nothing to do
            TrainModel(trainingPairs);
            if (!StockpileTrainingData)
                trainingPairs.Clear();
            return Enumerable.Empty<IWeightChange>();
        }

        /// <summary>Performs a training operation, but doesn't update any weights outside the model</summary>
        protected void TrainModel(Dictionary<double[], double[]> pairs)
        {
            //LATER learning rate and loss function should probably be part of the trainer rather than the network
            model.LearningRate = NeuralLearningRate * (overallWeightings.Sum(o => o.OverallWeight) + suitabilityRequirements.Sum(r => r.SuitabilityWeight));
            model.LossFunction = NeuralLossFunction;
            var trainer = new ModelTrainer(model)
            {
                LossThreshold = 0.005,
                MaxIterations = MaxTrainingIterations
            };
            var m = trainer.Train(pairs.Keys, pairs.Values);
        }
        #endregion

        #region Neural structure
        private double[] InputValues(Applicant a, Applicant b, Role role)
        {
            var values = new double[nInputs];
            var offset = nInputs / 2;
            var i = 0;
            foreach (var ow in overallWeightings)
            {
                if (ow is not Requirement requirement || role.Requirements.Contains(requirement))
                {
                    values[i] = ApplicantEngine.OverallSuitability(a);
                    values[i + offset] = ApplicantEngine.OverallSuitability(b);
                }
                i++;
            }
            foreach (var requirement in suitabilityRequirements)
            {
                if (role.Requirements.Contains(requirement))
                {
                    values[i] = ApplicantEngine.SuitabilityOf(a, requirement);
                    values[i + offset] = ApplicantEngine.SuitabilityOf(b, requirement);
                }
                i++;
            }
            foreach (var cr in existingRoleRequirements)
            {
                if (role.Requirements.Contains((Requirement)cr))
                {
                    values[i] = CountRoles(a, cr.Criteria, role);
                    values[i + offset] = CountRoles(b, cr.Criteria, role);
                }
                i++;
            }
            return values;
        }
        #endregion

        #region Applicant comparison
        public ApplicantForRoleComparer ComparerFor(Role role)
            => new ApplicantForRoleComparer(this, role);

        int IComparer<(Applicant, Role)>.Compare((Applicant, Role) x, (Applicant, Role) y)
        {
            if (x.Item2 != y.Item2)
                throw new ArgumentException("Role must be common between the 2 values.");
            return Compare(x.Item1, y.Item1, x.Item2);
        }

        public int Compare(Applicant a, Applicant b, Role for_role)
        {
            if (a == null || b == null)
                throw new ArgumentNullException();
            var a_better_than_b = model.Predict(InputValues(a, b, for_role))[0];
            if (a_better_than_b > 0.5)
                return 1; // A > B
            else if (a_better_than_b < 0.5)
                return -1; // A < B
            else // a_better_than_b == 0.5
                return 0; // A == B
        }

        public override double SuitabilityOf(Applicant applicant, Role role) => throw new NotImplementedException(); //TODO how will this work?
        #endregion

        #region Helper methods
        private static void EnsurePositive(ref double value, double minimum_magnitude = 0.01) => LimitValue(ref value, min: minimum_magnitude);
        private static void EnsureNegative(ref double value, double minimum_magnitude = 0.01) => LimitValue(ref value, max: -minimum_magnitude);

        private static void LimitValue(ref double value, double? min = null, double? max = null)
        {
            if (min.HasValue && value < min)
                value = min.Value;
            else if (max.HasValue && value > max)
                value = max.Value;
        }

        private static double[] NormaliseWeights(double[] raw_weights, double weight_ratio)
            => raw_weights.Select(w => w / weight_ratio).ToArray();

        /// <summary>Find the average of the matching pairs between the first half of the
        /// Neuron weights and the second half. Returned array will have half the length.</summary>
        private static double[] AverageOfPairedWeights(Neuron neuron)
        {
            var averages = new double[neuron.Weights.Length / 2];
            for (var n = 0; n < averages.Length; n++)
                averages[n] = (neuron.Weights[n] + -neuron.Weights[n + averages.Length]) / 2;
            return averages;
        }

        private (double[], double[], double[]) SplitWeights(double[] weights)
        {
            var results = weights.Split(new[] { overallWeightings.Length, suitabilityRequirements.Length, existingRoleRequirements.Length });
            return (results[0], results[1], results[2]);
        }

        private void EnsureCorrectPolarities(Neuron neuron)
        {
            neuron.Bias = 0;
            var offset = nInputs / 2;
            var i = 0;
            foreach (var ow in overallWeightings)
            {
                EnsurePositive(ref neuron.Weights[i]);
                EnsureNegative(ref neuron.Weights[i + offset]);
                i++;
            }
            foreach (var requirement in suitabilityRequirements)
            {
                EnsurePositive(ref neuron.Weights[i]);
                EnsureNegative(ref neuron.Weights[i + offset]);
                i++;
            }
            foreach (var requirement in existingRoleRequirements)
            {
                EnsureNegative(ref neuron.Weights[i], 0.001);
                EnsurePositive(ref neuron.Weights[i + offset], 0.001);
                i++;
            }
        }

        private void FlipPolarities(Neuron neuron)
        {
            neuron.Bias = 0;
            var offset = nInputs / 2;
            var i = 0;
            foreach (var ow in overallWeightings)
            {
                neuron.Weights[i + offset] *= -1;
                i++;
            }
            foreach (var requirement in suitabilityRequirements)
            {
                neuron.Weights[i + offset] *= -1;
                i++;
            }
            foreach (var requirement in existingRoleRequirements)
            {
                neuron.Weights[i] *= -1;
                i++;
            }
        }

        private double WeightIncreaseFactor(double[] raw_suitability_weights, double[] raw_overall_weights, Func<Requirement, bool> include_requirement)
        {
            double old_sum = 0;
            double new_sum = 0;
            for (var i = 0; i < overallWeightings.Length; i++)
            {
                if (overallWeightings[i] is not Requirement requirement || include_requirement(requirement))
                {
                    old_sum += overallWeightings[i].OverallWeight;
                    new_sum += raw_overall_weights[i];
                }
            }
            for (var i = 0; i < suitabilityRequirements.Length; i++)
            {
                if (include_requirement(suitabilityRequirements[i]))
                {
                    old_sum += suitabilityRequirements[i].SuitabilityWeight;
                    new_sum += raw_suitability_weights[i];
                }
            }
            return new_sum / old_sum;
        }
        #endregion
    }
}
