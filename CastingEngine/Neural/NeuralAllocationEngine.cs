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
        //TODO should these be settings?
        const int MAXIMUM_OVERALL_FACTOR_CHANGE = 10; // don't let the overall weight change by more than this factor in either direction

        readonly Requirement[] suitabilityRequirements;
        readonly ICriteriaRequirement[] existingRoleRequirements;
        readonly SingleLayerPerceptron model;
        readonly UserConfirmation confirm;
        readonly int nInputs;

        //LATER should these be abstracted into an interface maybe? something to enable arbitrary user settings to be set for an engine?
        #region Common with NeuralApplicantEngine (except for comments)
        /// <summary>The maximum number of training iterations run per invocation of
        /// <see cref="UserPickedCast(IEnumerable{Applicant}, IEnumerable{Applicant}, Role)"/></summary>
        public int MaxTrainingIterations { get; set; } = 10; //LATER make this a user setting

        /// <summary>The speed at which the neural network learns from results, as a fraction of the sum of
        /// <see cref="Requirement.SuitabilityWeight"/>. Reasonable values are between 0.001 and 0.01.
        /// WARNING: Changing this can have crazy consequences, slower is generally safer but be careful.</summary>
        public double NeuralLearningRate { get; set; } = 0.005; //LATER make this a user setting
        #endregion

        public NeuralAllocationEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, Criteria[] criterias, Requirement[] requirements, UserConfirmation confirm)
            : base(applicant_engine, alternative_casts, criterias)
        {
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
            model = new SingleLayerPerceptron(nInputs, 1, new Sigmoid(), new ClassificationError { Threshold = 0.5 }); //TODO vary classification error threshold if required
            LoadWeights();
        }

        private void LoadWeights()
        {
            var neuron = model.Layer.Neurons[0];
            neuron.Bias = 0;
            var offset = nInputs / 2;
            var i = 0;
            neuron.Weights[i] = 1;
            neuron.Weights[i + offset] = -1;
            foreach (var requirement in suitabilityRequirements)
            {
                i++;
                neuron.Weights[i] = requirement.SuitabilityWeight;
                neuron.Weights[i + offset] = -requirement.SuitabilityWeight;
            }
            foreach (var requirement in existingRoleRequirements)
            {
                i++;
                neuron.Weights[i] = -requirement.ExistingRoleCost / 100;
                neuron.Weights[i + offset] = requirement.ExistingRoleCost / 100;
            }
        }

        private double[] InputValues(Applicant a, Applicant b, Role role)
        {
            var values = new double[nInputs];
            var offset = nInputs / 2;
            var i = 0;
            var excluding_criterias = role.Requirements.OfType<ICriteriaRequirement>().Select(r => r.Criteria).ToHashSet();
            values[i] = OverallSuitabilityHack(a, excluding_criterias);
            values[i + offset] = OverallSuitabilityHack(b, excluding_criterias);
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

        private double OverallSuitabilityHack(Applicant applicant, HashSet<Criteria> excluding_criterias)
        {
            double max = ApplicantEngine.MaxOverallAbility;
            double min = ApplicantEngine.MinOverallAbility;
            double sum = 0;
            foreach (var a in applicant.Abilities)
            {
                if (!excluding_criterias.Contains(a.Criteria))
                    sum += (double)a.Mark / a.Criteria.MaxMark * a.Criteria.Weight;
                else if (a.Criteria.Weight > 0)
                    max -= a.Criteria.Weight;
                else if (a.Criteria.Weight < 0)
                    min -= a.Criteria.Weight;
            }
            if (sum == 0)
                return 0; // if sum is 0, (max-min) might be 0
            return (sum - min) / (max - min);
        }

        public override double SuitabilityOf(Applicant applicant, Role role)
        {
            double score = ApplicantEngine.OverallSuitability(applicant); // between 0 and 1 inclusive
            double max = 1;
            foreach (var requirement in suitabilityRequirements)
                if (role.Requirements.Contains(requirement))
                {
                    score += requirement.SuitabilityWeight * ApplicantEngine.SuitabilityOf(applicant, requirement);
                    max += requirement.SuitabilityWeight;
                }
            foreach (var requirement in existingRoleRequirements)
                if (role.Requirements.Contains((Requirement)requirement))
                    score -= requirement.ExistingRoleCost / 100 * CountRoles(applicant, requirement.Criteria, role);
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
            var training_pairs = new Dictionary<double[], double[]>();
            foreach (var (picked, not_picked) in NeuralApplicantEngine.ComparablePairs(applicants_picked, not_picked_array))
            {
                training_pairs.Add(InputValues(picked, not_picked, role), new[] { 1.0 });
                training_pairs.Add(InputValues(not_picked, picked, role), new[] { 0.0 });
            }
            if (training_pairs.Count == 0)
                return; // nothing to do
            // Train the model
            model.LearningRate = NeuralLearningRate * suitabilityRequirements.Sum(r => r.SuitabilityWeight);
            var trainer = new ModelTrainer(model)
            {
                LossThreshold = 0.005,
                MaxIterations = MaxTrainingIterations
            };
            var m = trainer.Train(training_pairs.Keys, training_pairs.Values);
            UpdateWeights(role);
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

        private static void LimitValue(ref double value, double min, double? max = null)
        {
            if (value < min)
                value = min;
            else if (max.HasValue && value > max)
                value = max.Value;
        }

        private static void DeprecatedLimitOverall(ref double overall, double _, double __) //TODO remove this (replace with LimitValue)
        {
            if (overall <= 0)
                overall = 1;
        }

        private static void DeprecatedLimitFromZero(ref double value, double min, double? max = null) //TODO remove this (replace with LimitValue)
        {
            if (value <= 0)
                value = min;
            else if (max.HasValue && value > max)
                value = max.Value;
        }

        private double RelevantWeightIncreaseFactor(double[] raw_suitability_weights, Role role)
        {
            double old_sum = 0;
            double new_sum = 0;
            for (var i = 0; i < suitabilityRequirements.Length; i++)
            {
                if (role.Requirements.Contains(suitabilityRequirements[i]))
                {
                    // relevant requirement
                    old_sum += suitabilityRequirements[i].SuitabilityWeight;
                    new_sum += raw_suitability_weights[i];
                }
                else
                {
                    // irrelevant requirement
                    //TODO might need to do something here to include irrelavnt weights and/or overall weight of 1 (or new value?)
                }
            }
            return new_sum / old_sum;
        }

        private double[] NormaliseWeights(double[] raw_weights, double overall_weight)
            => raw_weights.Select(w => w / overall_weight).ToArray();

        private double[] MultiplyWeights(double[] weights, double factor)
            => weights.Select(w => w * factor).ToArray();

        private void UpdateWeights(Role role)
        {
            var raw_weights = AverageOfPairedWeights(model.Layer.Neurons[0]);

            var (raw_overall_weight, raw_suitability_weights, raw_role_costs) = SplitWeights(raw_weights);

            DeprecatedLimitOverall(ref raw_overall_weight, 1 / MAXIMUM_OVERALL_FACTOR_CHANGE, MAXIMUM_OVERALL_FACTOR_CHANGE); //TODO what if the suitability weights also changed by a lot?

            var normalised_suitability_weights = NormaliseWeights(raw_suitability_weights, raw_overall_weight);

            var weight_ratio = RelevantWeightIncreaseFactor(raw_suitability_weights, role);

            var changes = new List<IWeightChange>();
            for (var i = 0; i < suitabilityRequirements.Length; i++)
            {
                var requirement = suitabilityRequirements[i];
                var new_weight = role.Requirements.Contains(requirement) ? normalised_suitability_weights[i] : (requirement.SuitabilityWeight * weight_ratio / raw_overall_weight);
                DeprecatedLimitFromZero(ref new_weight, 0.01);
                changes.Add(new SuitabilityWeightChange(requirement, new_weight));
            }

            var multiplied_role_costs = MultiplyWeights(raw_role_costs, -100); //TODO currently has to be done before normalisation otherwise results change wildly (see 1996)
            var normalised_role_costs = NormaliseWeights(multiplied_role_costs, raw_overall_weight);

            for (var i = 0; i < existingRoleRequirements.Length; i++)
            {
                var requirement = existingRoleRequirements[i];
                //TODO is the cost effectively the number of percetnage points TIMES the weight for that requirement? or TIMES the total weight? maybe thats why they look so high sometimes
                // - the cases that are failing in unit tests (accuracy goes backwards) seem to be when a cost maxes out at 100
                // - the math confirms: actual "cost of each role" in suitability (between 0 and 1) is -C/(1+W)
                var new_cost = role.Requirements.Contains((Requirement)requirement) ? normalised_role_costs[i] : (requirement.ExistingRoleCost * weight_ratio / raw_overall_weight);
                DeprecatedLimitFromZero(ref new_cost, 0.01, 100);
                changes.Add(new ExistingRoleCostChange(requirement, new_cost));
            }
            if (changes.Any(c => c.Significant))
            {
                var msg = "CARMEN's neural network has detected an improvement to the Requirement weights. Would you like to update them?";
                msg += "\nOverall Ability: 1.0";
                foreach (var change in changes.OrderBy(c => c.Requirement.Order))
                    msg += "\n" + change.Description;
                if (confirm(msg))
                    foreach (var change in changes)
                        change.Accept();
            }
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
