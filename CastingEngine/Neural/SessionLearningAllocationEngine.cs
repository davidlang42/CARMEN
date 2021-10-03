using Carmen.CastingEngine.Neural.Internal;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    public class SessionLearningAllocationEngine : NeuralAllocationEngine
    {
        readonly Dictionary<double[], double[]> trainingPairs = new();

        #region Engine parameters
        /// <summary>If true, training will occur whenever <see cref="UserPickedCast(IEnumerable{Applicant}, IEnumerable{Applicant}, Role)"/> is called</summary>
        public bool TrainImmediately { get; set; } = false;

        /// <summary>If true, training data will kept to be used again in future training</summary>
        public bool StockpileTrainingData { get; set; } = true;
        #endregion

        public SessionLearningAllocationEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, ShowRoot show_root, Requirement[] requirements, UserConfirmation confirm)
            : base(applicant_engine, alternative_casts, show_root, requirements, confirm)
        { }

        #region Business logic
        protected override IEnumerable<IWeightChange> TrainingPairsAdded(Dictionary<double[], double[]> pairs, Role role)
        {
            foreach (var pair in pairs)
                trainingPairs.Add(pair.Key, pair.Value);
            if (TrainImmediately)
                return TrainModel();
            else
                return Enumerable.Empty<IWeightChange>(); // do it later
        }

        protected override IEnumerable<IWeightChange> FinaliseTraining()
        {
            if (trainingPairs.Any())
                return TrainModel();
            else
                return Enumerable.Empty<IWeightChange>(); // nothing to do
        }

        /// <summary>Returns true if any changes are made to ShowModel objects</summary>
        private IEnumerable<IWeightChange> TrainModel()
        {
            //LATER learning rate and loss function should probably be part of the trainer rather than the network
            model.LearningRate = NeuralLearningRate * (showRoot.OverallSuitabilityWeight + suitabilityRequirements.Sum(r => r.SuitabilityWeight));
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
            var m = trainer.Train(trainingPairs.Keys, trainingPairs.Values);
            if (!StockpileTrainingData)
                trainingPairs.Clear();
            return CalculateChanges();
        }
        #endregion

        #region Neural structure
        private IEnumerable<IWeightChange> CalculateChanges()
        {
            EnsureCorrectPolarities(model.Layer.Neurons[0]);
            var raw_weights = AverageOfPairedWeights(model.Layer.Neurons[0]);

            var (raw_overall_weight, raw_suitability_weights, raw_role_weights) = SplitWeights(raw_weights);

            var new_sum = raw_overall_weight + raw_suitability_weights.Sum();
            var old_sum = showRoot.OverallSuitabilityWeight + suitabilityRequirements.Sum(r => r.SuitabilityWeight);
            var weight_ratio = new_sum / old_sum;

            var normalised_suitability_weights = NormaliseWeights(raw_suitability_weights, weight_ratio);
            var normalised_role_weights = NormaliseWeights(raw_role_weights, weight_ratio);

            var new_weights = new Dictionary<ICriteriaRequirement, double>();
            var changes = new List<IWeightChange>();
            if (includeOverall)
            {
                var normalised_overall_weight = raw_overall_weight / weight_ratio;
                changes.Add(new OverallWeightChange(showRoot, normalised_overall_weight));
            }
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

            return changes;
        }
        #endregion
    }
}
