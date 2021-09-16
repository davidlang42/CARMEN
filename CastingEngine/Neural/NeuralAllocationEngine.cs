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
    public class NeuralAllocationEngine : HeuristicAllocationEngine //LATER don't extend HeuristicAllocationEngine
    {
        readonly Criteria[] primaryCriterias;
        readonly Requirement[] requirements;
        readonly FeedforwardNetwork model;
        readonly int nInputs;

        public NeuralAllocationEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, Criteria[] criterias, Requirement[] requirements)
            : base(applicant_engine, alternative_casts, criterias)
        {
            this.primaryCriterias = criterias.InOrder().Where(c => c.Primary).ToArray();
            this.requirements = requirements.InOrder().ToArray();
            //TODO needs some way to persist neural network model
            nInputs = primaryCriterias.Length + requirements.Length + 1;
            var n_hidden_layers = Math.Max(primaryCriterias.Length, requirements.Length);
            var n_neurons_per_hidden_layer = (primaryCriterias.Length + 1) * (requirements.Length + 1);
            model = new FeedforwardNetwork(nInputs, n_hidden_layers, n_neurons_per_hidden_layer, 1, new Tanh(), new Sigmoid()); //TODO try different hidden activation functions
        }

        public override double SuitabilityOf(Applicant applicant, Role role)
        {
            if (!role.Requirements.Any())
                return ApplicantEngine.OverallSuitability(applicant);
            var inputs = InputValues(applicant, role);
            return model.Predict(inputs)[0];
        }

        private double[] InputValues(Applicant applicant, Role role)
        {
            var values = new double[nInputs];
            var i = 0;
            values[i++] = ApplicantEngine.OverallSuitability(applicant);
            foreach (var criteria in primaryCriterias)
                values[i++] = CountRoles(applicant, criteria, role);
            foreach (var requirement in requirements)
                if (role.Requirements.Contains(requirement))
                    values[i++] = ApplicantEngine.SuitabilityOf(applicant, requirement); // else 0
            if (i != nInputs)
                throw new ApplicationException($"Incomplete set of input values: {i} != {nInputs}");
            return values;
        }

        public override void UserPickedCast(IEnumerable<Applicant> applicants, Role role)
        {
            //TODO train
        }
    }
}
