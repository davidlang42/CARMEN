﻿using Carmen.CastingEngine.Neural.Internal;
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
    public class RoleLearningAllocationEngine : NeuralAllocationEngine
    {
        public RoleLearningAllocationEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, ShowRoot show_root, Requirement[] requirements, UserConfirmation confirm)
            : base(applicant_engine, alternative_casts, show_root, requirements, confirm)
        { }

        /// <summary>Returns an empty sequence because training is always processed immediately.</summary>
        protected override IEnumerable<IWeightChange> FinaliseTraining() => Enumerable.Empty<IWeightChange>();

        protected override IEnumerable<IWeightChange> TrainingPairsAdded(Dictionary<double[], double[]> pairs, Role role)
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
            var m = trainer.Train(pairs.Keys, pairs.Values);
            return CalculateChanges(role);
        }

        private IEnumerable<IWeightChange> CalculateChanges(Role role)
        {
            EnsureCorrectPolarities(model.Layer.Neurons[0]);
            var raw_weights = AverageOfPairedWeights(model.Layer.Neurons[0]);

            var (raw_overall_weight, raw_suitability_weights, raw_role_weights) = SplitWeights(raw_weights);
            
            var weight_ratio = RelevantWeightIncreaseFactor(raw_suitability_weights, raw_overall_weight, role);//TODO this line

            var normalised_suitability_weights = NormaliseWeights(raw_suitability_weights, weight_ratio);
            var normalised_role_weights = NormaliseWeights(raw_role_weights, weight_ratio);

            var old_total_weight_sum = showRoot.OverallSuitabilityWeight + suitabilityRequirements.Sum(r => r.SuitabilityWeight);

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
                var new_weight = role.Requirements.Contains(requirement) ? normalised_suitability_weights[i] : requirement.SuitabilityWeight; //TODO this line
                if (requirement is ICriteriaRequirement criteria_requirement)
                    new_weights.Add(criteria_requirement, new_weight);
                changes.Add(new SuitabilityWeightChange(requirement, new_weight));
            }
            for (var i = 0; i < existingRoleRequirements.Length; i++)
            {
                var requirement = existingRoleRequirements[i];
                //TODO this line
                var new_cost = role.Requirements.Contains((Requirement)requirement) ? WeightToCost(normalised_role_weights[i], new_weights[requirement], old_total_weight_sum) // after normalisation, the sum of weights will be the same as it was before
                    : requirement.ExistingRoleCost;
                LimitValue(ref new_cost, 0.01, 100);
                changes.Add(new ExistingRoleCostChange(requirement, new_cost));
            }

            return changes;
        }

        private double RelevantWeightIncreaseFactor(double[] raw_suitability_weights, double raw_overall_weight, Role role)
        {
            double old_sum = showRoot.OverallSuitabilityWeight;
            double new_sum = raw_overall_weight;
            for (var i = 0; i < suitabilityRequirements.Length; i++)
            {
                if (role.Requirements.Contains(suitabilityRequirements[i])) // requirement is relevant to this role
                {
                    old_sum += suitabilityRequirements[i].SuitabilityWeight;
                    new_sum += raw_suitability_weights[i];
                }
            }
            return new_sum / old_sum;
        }
    }
}
