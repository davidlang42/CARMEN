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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        #endregion

        public ComplexNeuralAllocationEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, ShowRoot show_root, Requirement[] requirements, UserConfirmation confirm)
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
            //TODO allow parameters to be configured (layers, neurons, hidden activation functions)
            model = new FeedforwardNetwork(nInputs, 2, nInputs * 2, 1, new Tanh(), new Sigmoid()); // sigmoid output is between 0 and 1, crossing at 0.5
            //TODO load network from file, or create fresh if required
        }

        #region Business logic
        public override bool UserPickedCast(IEnumerable<Applicant> applicants_picked, IEnumerable<Applicant> applicants_not_picked, Role role)
        {
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
            FinaliseTraining();
            //TODO persist model to disk
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
            model.LossFunction = NeuralLossFunction switch
            {
                LossFunctionChoice.MeanSquaredError => new MeanSquaredError(),
                LossFunctionChoice.Classification0_5 => new ClassificationError { Threshold = 0.5 },
                LossFunctionChoice.Classification0_4 => new ClassificationError { Threshold = 0.4 },
                LossFunctionChoice.Classification0_3 => new ClassificationError { Threshold = 0.3 },
                LossFunctionChoice.Classification0_2 => new ClassificationError { Threshold = 0.2 },
                LossFunctionChoice.Classification0_1 => new ClassificationError { Threshold = 0.1 },
                _ => throw new NotImplementedException($"Enum not implemented: {NeuralLossFunction}")
            };
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
