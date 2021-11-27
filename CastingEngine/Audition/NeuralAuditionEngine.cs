using Carmen.CastingEngine.Neural;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Audition
{
    /// <summary>
    /// An AuditionEngine which can learn from the user's choices, by training a SingleLayerPerceptron to update the criteria weights.
    /// </summary>
    public class NeuralAuditionEngine : WeightedSumEngine, IComparer<Applicant>
    {
        const double MINIMUM_CHANGE = 0.1;

        readonly SingleLayerPerceptron model;
        readonly Criteria[] criterias;
        readonly UserConfirmation confirm;

        /// <summary>The maximum number of training iterations run per invocation of
        /// <see cref="UserSelectedCast(IEnumerable{Applicant}, IEnumerable{Applicant})"/></summary>
        public int MaxTrainingIterations { get; set; } = 10;

        /// <summary>The speed at which the neural network learns from results, as a fraction of
        /// <see cref="MaxOverallAbility"/>. Reasonable values are between 0.001 and 0.01.
        /// WARNING: Changing this can have crazy consequences, slower is generally safer but be careful.</summary>
        public double NeuralLearningRate { get; set; } = 0.005;

        /// <summary>Determines which loss function is used when training the neural network.</summary>
        public LossFunctionChoice NeuralLossFunction { get; set; } = LossFunctionChoice.Classification0_3;

        public NeuralAuditionEngine(Criteria[] criterias, UserConfirmation confirm)
            : base(criterias)
        {
            this.criterias = criterias.Where(c => c.Weight != 0).ToArray(); // exclude criterias with zero weight
            if (this.criterias.Length == 0 && criterias.Length != 0)
            {
                // if all have zero weight, initialise them to equal parts of 100
                this.criterias = criterias;
                var equal_weight = 100.0 / criterias.Length;
                foreach (var criteria in this.criterias)
                    criteria.Weight = equal_weight;
            }
            this.confirm = confirm;
            this.model = new SingleLayerPerceptron(this.criterias.Length * 2, 1);
            LoadWeights();
        }

        private void LoadWeights()
        {
            var neuron = model.Layer.Neurons[0];
            neuron.Bias = 0;
            for (var i = 0; i < criterias.Length; i++)
            {
                neuron.Weights[i] = criterias[i].Weight;
                neuron.Weights[i + criterias.Length] = -criterias[i].Weight;
            }
        }

        public int Compare(Applicant? a, Applicant? b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException();
            var a_better_than_b = model.Predict(InputValues(a, b))[0];
            if (a_better_than_b > 0.5)
                return 1; // A > B
            else if (a_better_than_b < 0.5)
                return -1; // A < B
            else // a_better_than_b == 0.5
                return 0; // A == B
        }

        private double[] InputValues(Applicant a, Applicant b)
        {
            var values = new double[criterias.Length * 2];
            for (var i = 0; i < criterias.Length; i++)
            {
                double max_mark = criterias[i].MaxMark;
                values[i] = a.MarkFor(criterias[i]) / max_mark;
                values[i + criterias.Length] = b.MarkFor(criterias[i]) / max_mark;
            }
            return values;
        }

        public override async Task UserSelectedCast(IEnumerable<Applicant> applicants_accepted, IEnumerable<Applicant> applicants_rejected)
        {
            if (criterias.Length == 0)
                return; // nothing to do
            // Generate training data
            var rejected_array = applicants_rejected.ToArray();
            if (rejected_array.Length == 0)
                return; // nothing to do
            var training_pairs = await Task.Run(() => GenerateTrainingPairs(applicants_accepted, rejected_array));
            if (training_pairs.Count == 0)
                return; // nothing to do
            // Train the model
            model.LearningRate = NeuralLearningRate * MaxOverallAbility;
            model.LossFunction = NeuralLossFunction;
            var trainer = new ModelTrainer(model)
            {
                LossThreshold = 0.005,
                MaxIterations = MaxTrainingIterations,
            };
            await Task.Run(() => trainer.Train(training_pairs.Keys, training_pairs.Values));
            UpdateWeights();
        }

        private Dictionary<double[], double[]> GenerateTrainingPairs(IEnumerable<Applicant> applicants_accepted, Applicant[] rejected_array)
        {
            var pairs = new Dictionary<double[], double[]>();
            foreach (var (accepted, rejected) in ComparablePairs(applicants_accepted, rejected_array))
            {
                pairs.Add(InputValues(accepted, rejected), new[] { 1.0 });
                pairs.Add(InputValues(rejected, accepted), new[] { 0.0 });
            }
            return pairs;
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
                UpdateRange(criterias);
            }
            LoadWeights(); // revert minor or refused changes, update neurons with normalised weights
        }

        /// <summary>Finds pairs of good and bad applicants, where the bad applicant is eligible for the cast group of the good</summary>
        public static IEnumerable<(Applicant good, Applicant bad)> ComparablePairs(IEnumerable<Applicant> good_applicants, Applicant[] bad_applicants)
        {
            var good_by_group = good_applicants.GroupBy(a => a.CastGroup).ToDictionary(g => g.Key!, g => g.ToArray());
            var bad_by_group = new Dictionary<CastGroup, Applicant[]>();
            foreach (var cast_group in good_by_group.Keys)
                bad_by_group.Add(cast_group, bad_applicants
                    .Where(a => cast_group.Requirements.All(r => r.IsSatisfiedBy(a)))
                    .ToArray());
            foreach (var cg in good_by_group.Keys)
                foreach (var good in good_by_group[cg])
                    foreach (var bad in bad_by_group[cg])
                        yield return (good, bad);
        }
    }
}
