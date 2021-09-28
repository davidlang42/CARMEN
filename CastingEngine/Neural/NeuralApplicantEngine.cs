using Carmen.CastingEngine.Base;
using Carmen.CastingEngine.Neural.Internal;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    public class NeuralApplicantEngine : ApplicantEngine, IComparer<Applicant>
    {
        const double MINIMUM_CHANGE = 0.1;

        readonly SingleLayerPerceptron model;
        readonly Criteria[] criterias;
        readonly UserConfirmation confirm;

        int maxOverallAbility;
        public override int MaxOverallAbility => maxOverallAbility;

        int minOverallAbility;
        public override int MinOverallAbility => minOverallAbility;

        /// <summary>The maximum number of training iterations run per invocation of
        /// <see cref="UserSelectedCast(IEnumerable{Applicant}, IEnumerable{Applicant})"/></summary>
        public int MaxTrainingIterations { get; set; } = 10; //LATER make this a user setting

        /// <summary>The speed at which the neural network learns from results, as a fraction of
        /// <see cref="MaxOverallAbility"/>. Reasonable values are between 0.001 and 0.01.
        /// WARNING: Changing this can have crazy consequences, slower is generally safer but be careful.</summary>
        public double NeuralLearningRate { get; set; } = 0.005; //LATER make this a user setting

        /// Calculate the overall ability of an Applicant as a simple weighted sum of their Abilities</summary>
        public override int OverallAbility(Applicant applicant)
            => Convert.ToInt32(applicant.Abilities.Sum(a => (double)a.Mark / a.Criteria.MaxMark * a.Criteria.Weight));

        public NeuralApplicantEngine(Criteria[] criterias, UserConfirmation confirm)
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
            this.model = new SingleLayerPerceptron(this.criterias.Length * 2, 1, new Sigmoid(), new ClassificationError { Threshold = 0.25 });
            LoadWeights();
            UpdateRange();
        }

        private void UpdateRange()
        {
            var max = criterias.Select(c => c.Weight).Where(w => w > 0).Sum();
            if (max > int.MaxValue)
                throw new ApplicationException($"Sum of positive Criteria weights cannot exceed {int.MaxValue}: {max}");
            maxOverallAbility = Convert.ToInt32(max);
            var min = criterias.Select(c => c.Weight).Where(w => w < 0).Sum();
            if (min < int.MinValue)
                throw new ApplicationException($"Sum of negative Criteria weights cannot go below {int.MinValue}: {min}");
            minOverallAbility = Convert.ToInt32(min);
            if (minOverallAbility == maxOverallAbility) // == 0
                maxOverallAbility = 1; // to avoid division by zero errors
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

        public override void UserSelectedCast(IEnumerable<Applicant> applicants_accepted, IEnumerable<Applicant> applicants_rejected)
        {
            if (criterias.Length == 0)
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
