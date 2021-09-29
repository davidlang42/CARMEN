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
        const double MINIMUM_CHANGE = 0.1;

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
            nInputs = (suitabilityRequirements.Length + existingRoleRequirements.Length) * 2;
            model = new SingleLayerPerceptron(nInputs, 1, new Sigmoid(), new ClassificationError { Threshold = 0.5 });
            LoadWeights();
        }

        private void LoadWeights()
        {
            var neuron = model.Layer.Neurons[0];
            neuron.Bias = 0;
            var offset = nInputs / 2;
            var i = 0;
            foreach (var requirement in suitabilityRequirements)
            {
                neuron.Weights[i] = requirement.SuitabilityWeight;
                neuron.Weights[i + offset] = -requirement.SuitabilityWeight;
                i++;
            }
            foreach (var requirement in existingRoleRequirements)
            {
                neuron.Weights[i] = -requirement.ExistingRoleCost / 100;
                neuron.Weights[i + offset] = requirement.ExistingRoleCost / 100;
                i++;
            }
        }

        private double[] InputValues(Applicant a, Applicant b, Role role)
        {
            var values = new double[nInputs];
            var offset = nInputs / 2;
            var i = 0;
            foreach (var requirement in suitabilityRequirements)
            {
                if (role.Requirements.Contains(requirement))
                {
                    values[i] = ApplicantEngine.SuitabilityOf(a, requirement);
                    values[i + offset] = ApplicantEngine.SuitabilityOf(b, requirement);
                }
                i++;
            }
            foreach (var requirement in existingRoleRequirements)
            {
                if (role.Requirements.Contains((Requirement)requirement))
                {
                    values[i] = CountRoles(a, requirement.Criteria, role);
                    values[i + offset] = CountRoles(b, requirement.Criteria, role);
                }
                i++;
            }
            return values;
        }

        public override double SuitabilityOf(Applicant applicant, Role role)
        {
            double score = ApplicantEngine.OverallSuitability(applicant); // between 0 and 1 inclusive
            double max = 1;//TODO should this still include overal suitability?
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
            if (nInputs == 0)
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

        private void UpdateWeights(Role role)
        {
            var neuron = model.Layer.Neurons[0];
            var new_raw = new double[nInputs / 2];
            for (var n = 0; n < new_raw.Length; n++)
                new_raw[n] = (neuron.Weights[n] + -neuron.Weights[n + new_raw.Length]) / 2;
            var old_sum = suitabilityRequirements.Where(r => role.Requirements.Contains(r)).Sum(r => r.SuitabilityWeight);
            var new_sum = new_raw.Zip(suitabilityRequirements).Where(p => role.Requirements.Contains(p.Second)).Sum(p => p.First);
            var weight_ratio = old_sum / new_sum;
            var any_change = false;
            var i = 0;
            var changes = new List<WeightChange>();
            foreach (var requirement in suitabilityRequirements)
            {
                var new_weight = role.Requirements.Contains(requirement) ? new_raw[i] * weight_ratio : requirement.SuitabilityWeight;
                if (new_weight < 0)
                    new_weight = 0;
                string description;
                if (Math.Abs(new_weight - requirement.SuitabilityWeight) > MINIMUM_CHANGE)
                {
                    description = $"\n{requirement.Name}: {new_weight:0.0} (previously {requirement.SuitabilityWeight:0.0})";
                    any_change = true;
                }
                else
                    description = $"\n{requirement.Name}: {requirement.SuitabilityWeight:0.0}";
                changes.Add(new()
                {
                    Requirement = requirement,
                    Description = description,
                    Accept = () => requirement.SuitabilityWeight = new_weight
                });
                i++;
            }
            foreach (var requirement in existingRoleRequirements)
            {
                var new_cost = role.Requirements.Contains((Requirement)requirement) ? -100 * new_raw[i] : requirement.ExistingRoleCost;
                if (new_cost < 0)
                    new_cost = 0;
                if (new_cost > 100)
                    new_cost = 100;
                string description;
                if (Math.Abs(new_cost - requirement.ExistingRoleCost) > MINIMUM_CHANGE)
                {
                    description = $"\nEach '{requirement.Name}' role reduces suitability by: {new_cost:0.0}% (previously {requirement.ExistingRoleCost:0.0}%)";
                    any_change = true;
                }
                else
                    description = $"\nEach '{requirement.Name}' role reduces suitability by: {requirement.ExistingRoleCost:0.0}%";
                changes.Add(new()
                {
                    Requirement = requirement,
                    Description = description,
                    Accept = () => requirement.ExistingRoleCost = new_cost
                });
                i++;
            }
            if (any_change)
            {
                var msg = "CARMEN's neural network has detected an improvement to the Requirement weights. Would you like to update them?";
                foreach (var change in changes.OrderBy(c => c.Requirement.Order))
                    msg += "\n" + change.Description;
                if (confirm(msg))
                    foreach (var change in changes)
                        change.Accept();
            }
            LoadWeights(); // revert minor or refused changes, update neurons with normalised weights
        }

        private struct WeightChange
        {
            public IOrdered Requirement;
            public string Description;
            public Action Accept;
        }
    }
}
