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
    public class NeuralAllocationEngine : HeuristicAllocationEngine, IComparer<(Applicant, Role)> //LATER don't extend HeuristicAllocationEngine
    {
        readonly Requirement[] suitabilityRequirements;
        readonly ICriteriaRequirement[] existingRoleRequirements;
        readonly SingleLayerPerceptron model;
        readonly UserConfirmation confirm;
        readonly int nInputs;
        readonly Dictionary<double[], double[]> trainingPairs = new();

        //LATER should these be abstracted into an interface maybe? something to enable arbitrary user settings to be set for an engine?
        #region Common with NeuralApplicantEngine (except for comments)
        /// <summary>The maximum number of training iterations run per invocation of
        /// <see cref="UserPickedCast(IEnumerable{Applicant}, IEnumerable{Applicant}, Role)"/></summary>
        public int MaxTrainingIterations { get; set; } = 100; //LATER make this a user setting

        /// <summary>The speed at which the neural network learns from results, as a fraction of the sum of
        /// <see cref="Requirement.SuitabilityWeight"/>. Reasonable values are between 0.001 and 0.01.
        /// WARNING: Changing this can have crazy consequences, slower is generally safer but be careful.</summary>
        public double NeuralLearningRate { get; set; } = 0.005; //LATER make this a user setting
        #endregion

        public NeuralAllocationEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, Criteria[] criterias, Requirement[] requirements, UserConfirmation confirm)
            : base(applicant_engine, alternative_casts, criterias)
        {
            //TODO figure out how initialize it with reasonable defaults
            //TODO figure out how to extend / reduce an existing model for more / less requirements / etc
            //TODO make sure that learning is slow enough that values don't flip flop on every role change
            CountRolesByGeometricMean = true; //LATER shouldn't need to set these once this doesn't extend HeuristicAllocationEngine (they should be true by default)
            CountRolesIncludingPartialRequirements = true; //LATER shouldn't need to set these once this doesn't extend HeuristicAllocationEngine (they should be true by default)
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

        private void LoadWeights()
        {
            var neuron = model.Layer.Neurons[0];
            neuron.Bias = 0;
            var offset = nInputs / 2;
            var i = 0;
            neuron.Weights[i] = OverallWeight;
            neuron.Weights[i + offset] = -OverallWeight;
            foreach (var requirement in suitabilityRequirements)
            {
                i++;
                neuron.Weights[i] = requirement.SuitabilityWeight;
                neuron.Weights[i + offset] = -requirement.SuitabilityWeight;
            }
            foreach (var requirement in existingRoleRequirements)
            {
                i++;
                var weight = CostToNeuronWeight(requirement.ExistingRoleCost, ((Requirement)requirement).SuitabilityWeight);
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
            foreach (var requirement in existingRoleRequirements)
            {
                i++;
                if (role.Requirements.Contains((Requirement)requirement))
                {
                    values[i] = CountRoles(a, requirement.Criteria, role);
                    values[i + offset] = CountRoles(b, requirement.Criteria, role);
                }
            }
            return values;
        }

        public override double SuitabilityOf(Applicant applicant, Role role)
        {
            double score = ApplicantEngine.OverallSuitability(applicant); // between 0 and 1 inclusive
            double max = 1;//TODO use overall weight
            foreach (var requirement in suitabilityRequirements)
                if (role.Requirements.Contains(requirement))
                {
                    score += requirement.SuitabilityWeight * ApplicantEngine.SuitabilityOf(applicant, requirement);
                    max += requirement.SuitabilityWeight;
                }
            foreach (var requirement in existingRoleRequirements)
                if (role.Requirements.Contains((Requirement)requirement))
                    score -= requirement.ExistingRoleCost / 100 * CountRoles(applicant, requirement.Criteria, role); //TODO measure cost out of requirement suitability
            return score / max;
        }

        public ApplicantForRoleComparer ComparerFor(Role role)
            => new ApplicantForRoleComparer(this, role);

        public int Compare((Applicant, Role) x, (Applicant, Role) y)
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

        public override void UserPickedCast(IEnumerable<Applicant> applicants_picked, IEnumerable<Applicant> applicants_not_picked, Role role)
        {
            if (nInputs == 2) // no requirements, only overall suitability
                return; // nothing to do
            if (role.Requirements.Count(r => suitabilityRequirements.Contains(r)) == 0)
                return; // nothing to do
            // Generate training data
            var not_picked_array = applicants_not_picked.ToArray();
            if (not_picked_array.Length == 0)
                return; // nothing to do
            foreach (var (picked, not_picked) in NeuralApplicantEngine.ComparablePairs(applicants_picked, not_picked_array))
            {
                trainingPairs.Add(InputValues(picked, not_picked, role), new[] { 1.0 });
                trainingPairs.Add(InputValues(not_picked, picked, role), new[] { 0.0 });
            }
            if (TrainImmediately)
                TrainModel();
        }

        /// <summary>If true, <see cref="TrainModel"/> will be called whenever
        /// <see cref="UserPickedCast(IEnumerable{Applicant}, IEnumerable{Applicant}, Role)"/> is called</summary>
        public bool TrainImmediately { get; set; } = false; //LATER setting

        /// <summary>If true, training data will kept after calling <see cref="TrainModel"/>
        /// to be used again in future training</summary>
        public bool StockpileTrainingData { get; set; } = true; //LATER setting

        public ReloadWeights ReloadWeights { get; set; } = ReloadWeights.OnlyWhenRefused; //LATER setting

        public void TrainModel() //TODO make CarmenUI call TrainModel() at some point, or Dispose or something
        {
            if (trainingPairs.Count == 0)
                return; // nothing to do
            model.LearningRate = NeuralLearningRate * suitabilityRequirements.Sum(r => r.SuitabilityWeight);
            var trainer = new ModelTrainer(model)
            {
                LossThreshold = 0.005,
                MaxIterations = MaxTrainingIterations
            };
            var m = trainer.Train(trainingPairs.Keys, trainingPairs.Values);
            UpdateWeights();
            if (!StockpileTrainingData)
                trainingPairs.Clear();
        }

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

        private static void LimitValue(ref double value, double? min = null, double? max = null)
        {
            if (min.HasValue && value < min)
                value = min.Value;
            else if (max.HasValue && value > max)
                value = max.Value;
        }

        private double[] NormaliseWeights(double[] raw_weights, double weight_ratio)
            => raw_weights.Select(w => w / weight_ratio).ToArray();

        private static double CostToNeuronWeight(double cost, double suitability_weight)
            => -cost * suitability_weight / 100;

        private static double NeuronWeightToCost(double neuron_weight, double suitability_weight)
            => -neuron_weight / suitability_weight * 100;

        public double OverallWeight { get; set; } = 1; //TODO persist this in show model

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

        private void EnsurePositive(ref double value, double minimum_magnitude = 0.01) => LimitValue(ref value, min: minimum_magnitude);
        private void EnsureNegative(ref double value, double minimum_magnitude = 0.01) => LimitValue(ref value, max: -minimum_magnitude);

        private void UpdateWeights()
        {
            EnsureCorrectPolarities(model.Layer.Neurons[0]);
            var raw_weights = AverageOfPairedWeights(model.Layer.Neurons[0]);

            var (raw_overall_weight, raw_suitability_weights, raw_role_weights) = SplitWeights(raw_weights);

            var new_sum = raw_overall_weight + raw_suitability_weights.Sum();
            var old_sum = OverallWeight + suitabilityRequirements.Sum(r => r.SuitabilityWeight);
            var weight_ratio = new_sum / old_sum;

            var normalised_suitability_weights = NormaliseWeights(raw_suitability_weights, weight_ratio);
            var normalised_overall_weight = raw_overall_weight / weight_ratio;
            
            var new_weights = new Dictionary<ICriteriaRequirement, double>();

            var changes = new List<IWeightChange>
            {
                new OverallWeightChange(this, normalised_overall_weight)
            };
            for (var i = 0; i < suitabilityRequirements.Length; i++)
            {
                var requirement = suitabilityRequirements[i];
                var new_weight = normalised_suitability_weights[i];
                if (requirement is ICriteriaRequirement criteria_requirement)
                    new_weights.Add(criteria_requirement, new_weight);
                changes.Add(new SuitabilityWeightChange(requirement, new_weight));
            }

            var normalised_role_weights = NormaliseWeights(raw_role_weights, weight_ratio);

            for (var i = 0; i < existingRoleRequirements.Length; i++)
            {
                var requirement = existingRoleRequirements[i];
                var new_cost = NeuronWeightToCost(normalised_role_weights[i], new_weights[requirement]);
                LimitValue(ref new_cost, 0.01, 100);
                changes.Add(new ExistingRoleCostChange(requirement, new_cost));
            }

            if (changes.Any(c => c.Significant))
            {
                var msg = "CARMEN's neural network has detected an improvement to the Requirement weights. Would you like to update them?";
                foreach (var change in changes.OrderBy(c => c.Requirement.Order))
                    msg += "\n" + change.Description;
                if (confirm(msg))
                    foreach (var change in changes)
                        change.Accept();
                else if (ReloadWeights == ReloadWeights.OnlyWhenRefused)
                    LoadWeights(); // revert refused changes
                if (ReloadWeights == ReloadWeights.OnChange)
                    LoadWeights(); // revert refused changes, update neurons with normalised weights
            }
            if (ReloadWeights == ReloadWeights.Always)
                LoadWeights(); // revert minor or refused changes, update neurons with normalised weights
        }

        private interface IWeightChange
        {
            protected const double MINIMUM_CHANGE = 0.1;

            public IOrdered Requirement { get; }
            public string Description { get; }
            public bool Significant { get; }
            public void Accept();
        }

        public class OverallWeightChange : IWeightChange, IOrdered
        {
            readonly NeuralAllocationEngine engine;
            readonly double newWeight;

            public IOrdered Requirement { get; init; }

            public string Description => Significant
                ? $"Overall Ability: {newWeight:0.0} (previously {engine.OverallWeight:0.0})"
                : $"Overall Ability: {engine.OverallWeight:0.0}";

            public bool Significant { get; init; }

            int IOrdered.Order
            {
                get => int.MinValue;
                set => throw new NotImplementedException();
            }

            public void Accept()
            {
                engine.OverallWeight = newWeight;
            }

            public OverallWeightChange(NeuralAllocationEngine engine, double new_weight)
            {
                Requirement = this;
                this.engine = engine;
                newWeight = new_weight;
                Significant = Math.Abs(newWeight - engine.OverallWeight) > IWeightChange.MINIMUM_CHANGE;
            }
        }

        public class SuitabilityWeightChange : IWeightChange
        {
            readonly Requirement requirement;
            readonly double newWeight;

            public IOrdered Requirement => requirement;
            public string Description => Significant
                ? $"{requirement.Name}: {newWeight:0.0} (previously {requirement.SuitabilityWeight:0.0})"
                : $"{requirement.Name}: {requirement.SuitabilityWeight:0.0}";
            public bool Significant { get; init; }

            public SuitabilityWeightChange(Requirement requirement, double new_weight)
            {
                this.requirement = requirement;
                newWeight = new_weight;
                Significant = Math.Abs(newWeight - requirement.SuitabilityWeight) > IWeightChange.MINIMUM_CHANGE;
            }

            public void Accept()
            {
                requirement.SuitabilityWeight = newWeight;
            }
        }

        public class ExistingRoleCostChange : IWeightChange
        {
            readonly ICriteriaRequirement requirement;
            readonly double newCost;

            public IOrdered Requirement => requirement;
            public string Description => Significant
                ? $"Each '{requirement.Name}' role reduces suitability by: {newCost:0.0}% (previously {requirement.ExistingRoleCost:0.0}%)"
                : $"Each '{requirement.Name}' role reduces suitability by: {requirement.ExistingRoleCost:0.0}%";
            public bool Significant { get; init; }

            public ExistingRoleCostChange(ICriteriaRequirement requirement, double new_cost)
            {
                this.requirement = requirement;
                newCost = new_cost;
                Significant = Math.Abs(newCost - requirement.ExistingRoleCost) > IWeightChange.MINIMUM_CHANGE;
            }

            public void Accept()
            {
                requirement.ExistingRoleCost = newCost;
            }
        }
    }
}
