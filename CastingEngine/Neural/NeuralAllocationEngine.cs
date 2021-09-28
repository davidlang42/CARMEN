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

        //LATER should these be abstracted into an interface maybe? something to enable arbitrary user settings to be set for an engine?
        #region Common with NeuralApplicantEngine
        /// <summary>The maximum number of training iterations run per invocation of
        /// <see cref="UserSelectedCast(IEnumerable{Applicant}, IEnumerable{Applicant})"/></summary>
        public int MaxTrainingIterations { get; set; } = 10; //LATER make this a user setting

        /// <summary>The speed at which the neural network learns from results, as a fraction of
        /// <see cref="MaxOverallAbility"/>. Reasonable values are between 0.001 and 0.01.
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
            if (suitabilityRequirements.Length == 0 && requirements.Length != 0)//TODO ?
            {
                // if all have zero weight, initialise them to 1
                suitabilityRequirements = requirements;
                foreach (var requirement in suitabilityRequirements)
                    requirement.SuitabilityWeight = 1;
            }
            // Find the requirements which will detract based on existing roles
            existingRoleRequirements = requirements.OfType<ICriteriaRequirement>().Where(r => r.ExistingRoleWeight != 0).ToArray(); // exclude requirements with zero weight
            if (existingRoleRequirements.Length == 0 && requirements.OfType<ICriteriaRequirement>().Any())//TODO ?
            {
                // if all have zero weight, initialise them to -0.01
                existingRoleRequirements = requirements.OfType<ICriteriaRequirement>().ToArray();
                foreach (var requirement in existingRoleRequirements)
                    requirement.ExistingRoleWeight = -0.01;
            }
            // Construct the model
            this.confirm = confirm;
            nInputs = (suitabilityRequirements.Length + existingRoleRequirements.Length + 1) * 2;
            model = new SingleLayerPerceptron(nInputs, 1, new Sigmoid(), new ClassificationError { Threshold = 0.25 }); //TODO vary classification error threshold if required
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
                neuron.Weights[i] = requirement.SuitabilityWeight;
                neuron.Weights[i + offset] = -requirement.SuitabilityWeight;
                i++;
            }
            foreach (var requirement in existingRoleRequirements)
            {
                neuron.Weights[i] = requirement.ExistingRoleWeight;
                neuron.Weights[i + offset] = -requirement.ExistingRoleWeight;
                i++;
            }
        }

        private double[] InputValues(Applicant a, Applicant b, Role role)
        {
            var values = new double[nInputs];
            var offset = nInputs / 2;
            var i = 0;
            values[i] = ApplicantEngine.OverallAbility(a);
            values[i + offset] = ApplicantEngine.OverallAbility(b);
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
            double max = 1;
            foreach (var requirement in suitabilityRequirements)
                if (role.Requirements.Contains(requirement))
                {
                    score += requirement.SuitabilityWeight * ApplicantEngine.SuitabilityOf(applicant, requirement);
                    max += requirement.SuitabilityWeight;
                }
            foreach (var requirement in existingRoleRequirements)
                if (role.Requirements.Contains((Requirement)requirement))
                    score += requirement.ExistingRoleWeight * CountRoles(applicant, requirement.Criteria, role);
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

        public override void UserPickedCast(IEnumerable<Applicant> applicants_picked, IEnumerable<Applicant> applicants_not_picked, Role role)//TODO
        {
            if (nInputs == 2) // no requirements, only overall suitability
                return; // nothing to do
            // Generate training data
            var rejected_array = applicants_rejected.ToArray();
            if (rejected_array.Length == 0)
                return; // nothing to do
            var training_pairs = new Dictionary<double[], double[]>();
            foreach (var (accepted, rejected) in ComparablePairs(applicants_accepted, rejected_array))
            {
                training_pairs.Add(InputValues(accepted, rejected), new[] { 1.0 });
                training_pairs.Add(InputValues(rejected, accepted), new[] { 0.0 });
            }
            if (training_pairs.Count == 0)
                return; // nothing to do
            // Train the model
            model.LearningRate = NeuralLearningRate * MaxOverallAbility;
            var trainer = new ModelTrainer(model)
            {
                LossThreshold = 0.005,
                MaxIterations = MaxTrainingIterations
            };
            var m = trainer.Train(training_pairs.Keys, training_pairs.Values);
            UpdateWeights();
        }

        private void UpdateWeights()
        {
            var neuron = model.Layer.Neurons[0];
            var new_raw = new double[criterias.Length];
            double old_sum = 0;
            double new_sum = 0;
            for (var i = 0; i < criterias.Length; i++)
            {
                new_sum += new_raw[i] = (neuron.Weights[i] + -neuron.Weights[i + criterias.Length]) / 2;
                old_sum += criterias[i].Weight;
            }
            var weight_ratio = old_sum / new_sum;
            var new_weights = new double[criterias.Length];
            var any_change = false;
            var msg = "CARMEN's neural network has detected an improvement to the Criteria weights. Would you like to update them?";
            for (var i = 0; i < criterias.Length; i++)
            {
                new_weights[i] = new_raw[i] * weight_ratio;
                msg += $"\n{criterias[i].Name}: ";
                if (Math.Abs(new_weights[i] - criterias[i].Weight) > MINIMUM_CHANGE)
                {
                    msg += $"{new_weights[i]:0.0} (previously {criterias[i].Weight:0.0})";
                    any_change = true;
                }
                else
                    msg += $"{criterias[i].Weight:0.0}";
            }
            if (any_change && confirm(msg))
            {
                for (var i = 0; i < criterias.Length; i++)
                    criterias[i].Weight = new_weights[i];
                UpdateRange();
            }
            LoadWeights(); // revert minor or refused changes, update neurons with normalised weights
        }

        public static IEnumerable<(Applicant accepted, Applicant rejected)> ComparablePairs(IEnumerable<Applicant> applicants_accepted, Applicant[] applicants_rejected)
        {
            var accepted_by_group = applicants_accepted.GroupBy(a => a.CastGroup).ToDictionary(g => g.Key!, g => g.ToArray());
            var rejected_by_group = new Dictionary<CastGroup, Applicant[]>();
            foreach (var cast_group in accepted_by_group.Keys)
                rejected_by_group.Add(cast_group, applicants_rejected
                    .Where(a => cast_group.Requirements.All(r => r.IsSatisfiedBy(a)))
                    .ToArray());
            foreach (var cg in accepted_by_group.Keys)
                foreach (var accepted in accepted_by_group[cg])
                    foreach (var rejected in rejected_by_group[cg])
                        yield return (accepted, rejected);
        }
    }
}
