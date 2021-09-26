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
        SingleLayerPerceptron network;
        Criteria[] criterias;

        int maxOverallAbility;
        public override int MaxOverallAbility => maxOverallAbility;

        int minOverallAbility;
        public override int MinOverallAbility => minOverallAbility;

        /// Calculate the overall ability of an Applicant as a simple weighted sum of their Abilities</summary>
        public override int OverallAbility(Applicant applicant)
            => Convert.ToInt32(applicant.Abilities.Sum(a => (double)a.Mark / a.Criteria.MaxMark * a.Criteria.Weight));

        public NeuralApplicantEngine(Criteria[] criterias)
        {
            this.criterias = criterias;
            this.network = new SingleLayerPerceptron(criterias.Length * 2, 1);
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
            var neuron = network.Layer.Neurons[0];
            neuron.Bias = 0;
            for (var i = 0; i < criterias.Length; i++)
            {
                neuron.Weights[i] = criterias[i].Weight;
                neuron.Weights[i + criterias.Length] = -criterias[i].Weight;
            }
        }

        private void SaveWeights()
        {
            var neuron = network.Layer.Neurons[0];
            for (var i = 0; i < criterias.Length; i++)
                criterias[i].Weight = (neuron.Weights[i] + -neuron.Weights[i + criterias.Length]) / 2;
        }

        public int Compare(Applicant? a, Applicant? b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException();
            var a_better_than_b = network.Predict(InputValues(a, b))[0];
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
            //TODO train network
        }
    }
}
