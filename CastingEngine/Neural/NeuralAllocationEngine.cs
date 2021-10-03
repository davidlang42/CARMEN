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
    public class NeuralAllocationEngine : WeightedAverageEngine, IComparer<(Applicant, Role)>
    {
        readonly Requirement[] suitabilityRequirements;
        readonly ICriteriaRequirement[] existingRoleRequirements;
        readonly SingleLayerPerceptron model;
        readonly UserConfirmation confirm;
        readonly int nInputs;
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

        /// <summary>If true, <see cref="TrainModel"/> will be called whenever
        /// <see cref="UserPickedCast(IEnumerable{Applicant}, IEnumerable{Applicant}, Role)"/> is called</summary>
        public bool TrainImmediately { get; set; } = false;

        /// <summary>If true, training data will kept after calling <see cref="TrainModel"/>
        /// to be used again in future training</summary>
        public bool StockpileTrainingData { get; set; } = true;

        /// <summary>Determines when the updated weights are reloaded into the neural network.</summary>
        public ReloadWeights ReloadWeights { get; set; } = ReloadWeights.OnlyWhenRefused;
        #endregion

        public NeuralAllocationEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, ShowRoot show_root, Requirement[] requirements, UserConfirmation confirm)
            : base(applicant_engine, alternative_casts, show_root)
        {
            // Find the requirements which will be used for role suitability
            suitabilityRequirements = requirements.Where(r => r.SuitabilityWeight != 0).ToArray(); // exclude requirements with zero weight
            if (suitabilityRequirements.Length == 0 && requirements.Length != 0)
            {
                // if all have zero weight, initialise them to 1
                suitabilityRequirements = requirements;
                foreach (var requirement in suitabilityRequirements)
                    requirement.SuitabilityWeight = 1; // equal weighting to overall suitability
            }
            // Find the requirements which will detract based on existing roles
            existingRoleRequirements = requirements.OfType<ICriteriaRequirement>().Where(r => r.ExistingRoleCost != 0).ToArray(); // exclude requirements with zero weight
            if (existingRoleRequirements.Length == 0 && requirements.OfType<ICriteriaRequirement>().Any())
            {
                // if all have zero weight, initialise them to 1%
                existingRoleRequirements = requirements.OfType<ICriteriaRequirement>().ToArray();
                foreach (var requirement in existingRoleRequirements)
                    requirement.ExistingRoleCost = 1; // each role subtracts 1% suitability
            }
            // Construct the model
            this.confirm = confirm;
            nInputs = (suitabilityRequirements.Length + existingRoleRequirements.Length + 1) * 2;
            model = new SingleLayerPerceptron(nInputs, 1, new Sigmoid(), new ClassificationError { Threshold = 0.4 });
            LoadWeights();
        }

        #region Business logic
        public override bool UserPickedCast(IEnumerable<Applicant> applicants_picked, IEnumerable<Applicant> applicants_not_picked, Role role)
        {
            if (nInputs == 2) // no requirements, only overall suitability
                return false; // nothing to do
            if (role.Requirements.Count(r => suitabilityRequirements.Contains(r)) == 0)
                return false; // nothing to do
            // Generate training data
            var not_picked_array = applicants_not_picked.ToArray();
            if (not_picked_array.Length == 0)
                return false; // nothing to do
            foreach (var (picked, not_picked) in NeuralApplicantEngine.ComparablePairs(applicants_picked, not_picked_array))
            {
                trainingPairs.Add(InputValues(picked, not_picked, role), new[] { 1.0 });
                trainingPairs.Add(InputValues(not_picked, picked, role), new[] { 0.0 });
            }
            if (TrainImmediately)
                return TrainModel();
            else
                return false;
        }

        /// <summary>Returns true if any changes are made to ShowModel objects</summary>
        public override bool Finalise() => TrainModel();

        /// <summary>Returns true if any changes are made to ShowModel objects</summary>
        public bool TrainModel()
        {
            if (trainingPairs.Count == 0)
                return false; // nothing to do
            model.LearningRate = NeuralLearningRate * (showRoot.OverallSuitabilityWeight + suitabilityRequirements.Sum(r => r.SuitabilityWeight));
            var trainer = new ModelTrainer(model)
            {
                LossThreshold = 0.005,
                MaxIterations = MaxTrainingIterations
            };
            var m = trainer.Train(trainingPairs.Keys, trainingPairs.Values);
            if (!StockpileTrainingData)
                trainingPairs.Clear();
            return UpdateWeights();
        }
        #endregion

        #region Neural structure
        private void LoadWeights()
        {
            var neuron = model.Layer.Neurons[0];
            neuron.Bias = 0;
            var offset = nInputs / 2;
            var i = 0;
            neuron.Weights[i] = showRoot.OverallSuitabilityWeight;
            neuron.Weights[i + offset] = -showRoot.OverallSuitabilityWeight;
            double weight_sum = showRoot.OverallSuitabilityWeight;
            foreach (var requirement in suitabilityRequirements)
            {
                i++;
                neuron.Weights[i] = requirement.SuitabilityWeight;
                neuron.Weights[i + offset] = -requirement.SuitabilityWeight;
                weight_sum += requirement.SuitabilityWeight;
            }
            foreach (var requirement in existingRoleRequirements)
            {
                i++;
                var weight = CostToWeight(requirement.ExistingRoleCost, requirement.SuitabilityWeight, weight_sum);
                neuron.Weights[i] = weight;
                neuron.Weights[i + offset] = -weight;
            }
        }

        private double[] InputValues(Applicant a, Applicant b, Role role)
        {
            var values = new double[nInputs];
            var offset = nInputs / 2;
            var i = 0;
            values[i] = ApplicantEngine.OverallSuitability(a);
            values[i + offset] = ApplicantEngine.OverallSuitability(b);
            foreach (var requirement in suitabilityRequirements)
            {
                i++;
                if (role.Requirements.Contains(requirement))
                {
                    values[i] = ApplicantEngine.SuitabilityOf(a, requirement);
                    values[i + offset] = ApplicantEngine.SuitabilityOf(b, requirement);
                }
            }
            foreach (var cr in existingRoleRequirements)
            {
                i++;
                if (role.Requirements.Contains((Requirement)cr))
                {
                    values[i] = CountRoles(a, cr.Criteria, role);
                    values[i + offset] = CountRoles(b, cr.Criteria, role);
                }
            }
            return values;
        }

        private bool UpdateWeights()
        {
            EnsureCorrectPolarities(model.Layer.Neurons[0]);
            var raw_weights = AverageOfPairedWeights(model.Layer.Neurons[0]);

            var (raw_overall_weight, raw_suitability_weights, raw_role_weights) = SplitWeights(raw_weights);

            var new_sum = raw_overall_weight + raw_suitability_weights.Sum();
            var old_sum = showRoot.OverallSuitabilityWeight + suitabilityRequirements.Sum(r => r.SuitabilityWeight);
            var weight_ratio = new_sum / old_sum;

            var normalised_suitability_weights = NormaliseWeights(raw_suitability_weights, weight_ratio);
            var normalised_overall_weight = raw_overall_weight / weight_ratio;
            var normalised_role_weights = NormaliseWeights(raw_role_weights, weight_ratio);

            var new_weights = new Dictionary<ICriteriaRequirement, double>();
            var changes = new List<IWeightChange>
            {
                new OverallWeightChange(showRoot, normalised_overall_weight)
            };
            for (var i = 0; i < suitabilityRequirements.Length; i++)
            {
                var requirement = suitabilityRequirements[i];
                var new_weight = normalised_suitability_weights[i];
                if (requirement is ICriteriaRequirement criteria_requirement)
                    new_weights.Add(criteria_requirement, new_weight);
                changes.Add(new SuitabilityWeightChange(requirement, new_weight));
            }
            for (var i = 0; i < existingRoleRequirements.Length; i++)
            {
                var requirement = existingRoleRequirements[i];
                var new_cost = WeightToCost(normalised_role_weights[i], new_weights[requirement], old_sum); // after normalisation, the sum of weights will be the same as it was before
                LimitValue(ref new_cost, 0.01, 100);
                changes.Add(new ExistingRoleCostChange(requirement, new_cost));
            }

            bool show_model_updated = false;

            if (changes.Any(c => c.Significant))
            {
                var msg = "CARMEN's neural network has detected an improvement to the Requirement weights. Would you like to update them?";
                foreach (var change in changes.OrderBy(c => c.Requirement.Order))
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

        private (double, double[], double[]) SplitWeights(double[] weights)
        {
            var results = weights.Split(new[] { 1, suitabilityRequirements.Length, existingRoleRequirements.Length });
            return (results[0][0], results[1], results[2]);
        }

        private void EnsureCorrectPolarities(Neuron neuron)
        {
            neuron.Bias = 0;
            var offset = nInputs / 2;
            var i = 0;
            EnsurePositive(ref neuron.Weights[i]);
            EnsureNegative(ref neuron.Weights[i + offset]);
            foreach (var requirement in suitabilityRequirements)
            {
                i++;
                EnsurePositive(ref neuron.Weights[i]);
                EnsureNegative(ref neuron.Weights[i + offset]);
            }
            foreach (var requirement in existingRoleRequirements)
            {
                i++;
                EnsureNegative(ref neuron.Weights[i], 0.001);
                EnsurePositive(ref neuron.Weights[i + offset], 0.001);
            }
        }
        #endregion
    }
}
