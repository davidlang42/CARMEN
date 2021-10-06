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
    /// The base class of all Neural Network based allocation engines
    /// </summary>
    public abstract class NeuralAllocationEngine : WeightedAverageEngine, IComparer<(Applicant, Role)>
    {
        readonly IOverallWeighting[] overallWeightings;
        readonly Requirement[] suitabilityRequirements;
        readonly ICriteriaRequirement[] existingRoleRequirements;
        readonly SingleLayerPerceptron model;
        readonly int nInputs;
        readonly UserConfirmation confirm;

        //LATER allow users to change these parameters
        #region Engine parameters
        /// <summary>The maximum number of training iterations run per invocation of
        /// <see cref="UserPickedCast(IEnumerable{Applicant}, IEnumerable{Applicant}, Role)"/></summary>
        public int MaxTrainingIterations { get; set; } = 100;

        /// <summary>The speed at which the neural network learns from results, as a fraction of the sum of
        /// <see cref="Requirement.SuitabilityWeight"/>. Reasonable values are between 0.001 and 0.01.
        /// WARNING: Changing this can have crazy consequences, slower is generally safer but be careful.</summary>
        public double NeuralLearningRate { get; set; } = 0.005;

        /// <summary>Determines when the updated weights are reloaded into the neural network.</summary>
        public ReloadWeights ReloadWeights { get; set; } = ReloadWeights.OnlyWhenRefused;

        /// <summary>Determines which loss function is used when training the neural network.</summary>
        public LossFunctionChoice NeuralLossFunction { get; set; } = LossFunctionChoice.Classification0_4;
        #endregion

        public NeuralAllocationEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, ShowRoot show_root, Requirement[] requirements, UserConfirmation confirm)
            : base(applicant_engine, alternative_casts, show_root)
        {
            // Find out what (and how many) things will have overall ability weights
            overallWeightings = (show_root.CommonOverallWeight.HasValue ? show_root.Yield() : requirements.OfType<IOverallWeighting>())
                .Where(ow => ow.OverallWeight != 0).ToArray(); // zero means disabled
            // Find the requirements which will be used for role suitability
            suitabilityRequirements = requirements.Where(r => r.SuitabilityWeight != 0).ToArray(); // zero means disabled
            // Find the requirements which will detract based on existing roles
            existingRoleRequirements = requirements.OfType<ICriteriaRequirement>().Where(r => r.SuitabilityWeight != 0 && r.ExistingRoleCost != 0).ToArray(); // zero means disabled
            // Construct the model
            this.confirm = confirm;
            nInputs = (overallWeightings.Length + suitabilityRequirements.Length + existingRoleRequirements.Length) * 2;
            model = new SingleLayerPerceptron(nInputs, 1, new Sigmoid()); // sigmoid output is between 0 and 1, crossing at 0.5
            LoadWeights();
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
            var changes = TrainingPairsAdded(training_pairs, role);
            if (changes.Any())
                return UpdateWeights(changes);
            else
                return false;
        }

        public override bool ExportChanges()
        {
            var changes = FinaliseTraining();
            if (changes.Any())
                return UpdateWeights(changes);
            else
                return false;
        }

        /// <summary>Handle the addition of new training pairs, returning suggested weight changes, if any</summary>
        protected abstract IEnumerable<IWeightChange> TrainingPairsAdded(Dictionary<double[], double[]> pairs, Role role);

        /// <summary>Handle any remaining training, returning suggested weight changes, if any</summary>
        protected abstract IEnumerable<IWeightChange> FinaliseTraining();

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
        private void LoadWeights()
        {
            var neuron = model.Layer.Neurons[0];
            neuron.Bias = 0;
            var offset = nInputs / 2;
            var i = 0;
            double weight_sum = 0;
            foreach (var ow in overallWeightings)
            {
                neuron.Weights[i] = ow.OverallWeight;
                neuron.Weights[i + offset] = -ow.OverallWeight;
                weight_sum += ow.OverallWeight;
                i++;
            }
            foreach (var requirement in suitabilityRequirements)
            {
                neuron.Weights[i] = requirement.SuitabilityWeight;
                neuron.Weights[i + offset] = -requirement.SuitabilityWeight;
                weight_sum += requirement.SuitabilityWeight;
                i++;
            }
            foreach (var requirement in existingRoleRequirements)
            {
                var weight = CostToWeight(requirement.ExistingRoleCost, requirement.SuitabilityWeight, weight_sum);
                neuron.Weights[i] = weight;
                neuron.Weights[i + offset] = -weight;
                i++;
            }
        }

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

        protected IEnumerable<IWeightChange> CalculateChanges(Func<Requirement, bool> is_relevant)
        {
            EnsureCorrectPolarities(model.Layer.Neurons[0]);
            var raw_weights = AverageOfPairedWeights(model.Layer.Neurons[0]);

            var (raw_overall_weights, raw_suitability_weights, raw_role_weights) = SplitWeights(raw_weights);

            var relevant_weight_ratio = WeightIncreaseFactor(raw_suitability_weights, raw_overall_weights, is_relevant);
            var total_weight_sum = overallWeightings.Sum(o => o.OverallWeight) + suitabilityRequirements.Sum(r => r.SuitabilityWeight);

            var normalised_suitability_weights = NormaliseWeights(raw_suitability_weights, relevant_weight_ratio);
            var normalised_role_weights = NormaliseWeights(raw_role_weights, relevant_weight_ratio);
            var normalised_overall_weights = NormaliseWeights(raw_overall_weights, relevant_weight_ratio);

            var new_weights = new Dictionary<ICriteriaRequirement, double>();
            var changes = new List<IWeightChange>();
            for (var i = 0; i < overallWeightings.Length; i++)
            {
                var ow = overallWeightings[i];
                var new_weight = ow is not Requirement requirement || is_relevant(requirement) ? normalised_overall_weights[i] : ow.OverallWeight;
                changes.Add(new OverallWeightChange(ow, new_weight));
            }
            for (var i = 0; i < suitabilityRequirements.Length; i++)
            {
                var requirement = suitabilityRequirements[i];
                var new_weight = is_relevant(requirement) ? normalised_suitability_weights[i] : requirement.SuitabilityWeight;
                if (requirement is ICriteriaRequirement criteria_requirement)
                    new_weights.Add(criteria_requirement, new_weight);
                changes.Add(new SuitabilityWeightChange(requirement, new_weight));
            }
            for (var i = 0; i < existingRoleRequirements.Length; i++)
            {
                var requirement = existingRoleRequirements[i];
                var new_cost = is_relevant((Requirement)requirement) ?
                    WeightToCost(normalised_role_weights[i], new_weights[requirement], total_weight_sum) : requirement.ExistingRoleCost;
                LimitValue(ref new_cost, 0.01, 100);
                changes.Add(new ExistingRoleCostChange(requirement, new_cost));
            }

            return changes;
        }

        private bool UpdateWeights(IEnumerable<IWeightChange> changes)
        {
            bool show_model_updated = false;

            if (changes.Any(c => c.Significant))
            {
                var msg = "CARMEN's neural network has detected an improvement to the Requirement weights. Would you like to update them?";
                foreach (var change in changes.InOrder())
                    msg += "\n" + change.Description;
                if (confirm(msg))
                {
                    foreach (var change in changes)
                        change.Accept();
                    show_model_updated = true;
                }
                else if (ReloadWeights == ReloadWeights.OnlyWhenRefused)
                    LoadWeights(); // revert refused changes
                if (ReloadWeights == ReloadWeights.OnChange)
                    LoadWeights(); // revert refused changes, update neurons with normalised weights
            }
            if (ReloadWeights == ReloadWeights.Always)
                LoadWeights(); // revert minor or refused changes, update neurons with normalised weights

            return show_model_updated;
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
