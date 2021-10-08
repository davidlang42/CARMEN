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
    /// <summary>
    /// The base class of all Neural Network based allocation engines
    /// </summary>
    public abstract class NeuralAllocationEngine : WeightedAverageEngine, IComparer<(Applicant, Role)>
    {
        protected readonly IOverallWeighting[] overallWeightings;
        protected readonly Requirement[] suitabilityRequirements;
        protected readonly ICriteriaRequirement[] existingRoleRequirements;
        protected readonly int nInputs;
        protected readonly UserConfirmation confirm;

        protected abstract INeuralNetwork Model { get; }

        //LATER allow users to change these parameters
        #region Engine parameters
        /// <summary>The maximum number of training iterations run per invocation of
        /// <see cref="UserPickedCast(IEnumerable{Applicant}, IEnumerable{Applicant}, Role)"/></summary>
        public int MaxTrainingIterations { get; set; } = 100;

        /// <summary>The speed at which the neural network learns from results, as a fraction of the sum of
        /// <see cref="Requirement.SuitabilityWeight"/>. Reasonable values are between 0.001 and 0.01.
        /// WARNING: Changing this can have crazy consequences, slower is generally safer but be careful.</summary>
        public double NeuralLearningRate { get; set; } = 0.005;

        /// <summary>Determines which loss function is used when training the neural network.</summary>
        public LossFunctionChoice NeuralLossFunction { get; set; } = LossFunctionChoice.Classification0_4;

        /// <summary>The sorting algorithm used for ordering the applicants with the neural network</summary>
        public SortAlgorithm SortAlgorithm { get; set; } = SortAlgorithm.OrderBySuitability;
        #endregion

        /// <param name="overall_weightings">The things which will provide overall ability weights</param>
        /// <param name="suitability_requirements">The requirements which will be used for role suitability weights</param>
        /// <param name="existing_role_requirements">The requirements which will detract based on existing roles</param>
        public NeuralAllocationEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, ShowRoot show_root,
            IOverallWeighting[] overall_weightings, Requirement[] suitability_requirements, ICriteriaRequirement[] existing_role_requirements,
            UserConfirmation confirm)
            : base(applicant_engine, alternative_casts, show_root)
        {
            overallWeightings = overall_weightings;
            suitabilityRequirements = suitability_requirements;
            existingRoleRequirements = existing_role_requirements;
            this.confirm = confirm;
            nInputs = (overallWeightings.Length + suitabilityRequirements.Length + existingRoleRequirements.Length) * 2;
            if (overallWeightings.Length + suitabilityRequirements.Length < 2 && !ConfirmEngineCantLearn())
                throw new ApplicationException("Not enough weights are enabled for the NeuralAllocationEngine to learn from."
                    + "\nPlease enable more Suitability and Overall weights on Requirements on the Configuring Show page."
                    + "\nThis error can be avoided by choosing a non-learning AllocationEngine in the Advanced settings."); //LATER make sure that when this is thrown, the user is taken cleanly back to the main menu
        }

        #region Business logic
        private bool ConfirmEngineCantLearn()
        {
            var msg = "There ";
            if (overallWeightings.Length + suitabilityRequirements.Length == 1)
                msg += "is currently only 1 Requirement";
            else
                msg += "are currently no Requirements";
            msg += " with 'Weight' enabled. This will not allow the CARMEN engine to learn from your casting choices.";
            msg += "\nWould you like to continue?";
            return confirm(msg);
        }

        public override void UserPickedCast(IEnumerable<Applicant> applicants_picked, IEnumerable<Applicant> applicants_not_picked, Role role)
        {
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
            if (!training_pairs.Any())
                return; // nothing to do
            // Process training data
            AddTrainingPairs(training_pairs, role);
        }

        public override void ExportChanges() => FinaliseTraining();

        /// <summary>Handle the addition of new training pairs, returning suggested weight changes, if any</summary>
        protected abstract void AddTrainingPairs(Dictionary<double[], double[]> pairs, Role role);

        /// <summary>Handle any remaining training, returning suggested weight changes, if any</summary>
        protected abstract void FinaliseTraining();
        #endregion

        #region Neural structure
        private double[] InputValues(Applicant a, Applicant b, Role role)
        {
            var values = new double[nInputs];
            var offset = nInputs / 2;
            var i = 0;
            foreach (var ow in overallWeightings)
            {
                if (ow is not Requirement requirement || role.Requirements.Contains(requirement))
                {
                    values[i] = ApplicantEngine.OverallSuitability(a);
                    values[i + offset] = ApplicantEngine.OverallSuitability(b);
                }
                i++;
            }
            foreach (var requirement in suitabilityRequirements)
            {
                if (role.Requirements.Contains(requirement))
                {
                    values[i] = ApplicantEngine.SuitabilityOf(a, requirement);
                    values[i + offset] = ApplicantEngine.SuitabilityOf(b, requirement);
                }
                i++;
            }
            foreach (var cr in existingRoleRequirements)
            {
                if (role.Requirements.Contains((Requirement)cr))
                {
                    values[i] = CountRoles(a, cr.Criteria, role);
                    values[i + offset] = CountRoles(b, cr.Criteria, role);
                }
                i++;
            }
            return values;
        }

        /// <summary>Performs a training operation, but doesn't update any weights outside the model</summary>
        protected void TrainModel(Dictionary<double[], double[]> pairs)
        {
            //LATER learning rate and loss function should probably be part of the trainer rather than the network
            Model.LearningRate = CalculateLearningRate();
            Model.LossFunction = NeuralLossFunction;
            var trainer = new ModelTrainer(Model)
            {
                LossThreshold = 0.005,
                MaxIterations = MaxTrainingIterations
            };
            var m = trainer.Train(pairs.Keys, pairs.Values);
        }

        protected virtual double CalculateLearningRate() => NeuralLearningRate;
        #endregion

        #region Applicant comparison
        public ApplicantForRoleComparer ComparerFor(Role role)
            => new ApplicantForRoleComparer(this, role);

        int IComparer<(Applicant, Role)>.Compare((Applicant, Role) x, (Applicant, Role) y)
        {
            if (x.Item2 != y.Item2)
                throw new ArgumentException("Role must be common between the 2 values.");
            return Compare(x.Item1, y.Item1, x.Item2);
        }

        public int Compare(Applicant a, Applicant b, Role for_role)
        {
            if (a == null || b == null)
                throw new ArgumentNullException();
            var a_better_than_b = Model.Predict(InputValues(a, b, for_role))[0];
            if (a_better_than_b > 0.5)
                return 1; // A > B
            else if (a_better_than_b < 0.5)
                return -1; // A < B
            else // a_better_than_b == 0.5
                return 0; // A == B
        }

        /// <summary>Use the neural network to order the applicants, based on the chosen SortingAlgorithm</summary>
        public override IEnumerable<Applicant> InPreferredOrder(IEnumerable<Applicant> applicants, Role role, bool reverse = false)
            => SortAlgorithm switch
            {
                SortAlgorithm.OrderBySuitability => base.InPreferredOrder(applicants, role, reverse),
                SortAlgorithm.OrderByComparer when reverse => applicants.OrderByDescending(a => a, ComparerFor(role)),
                SortAlgorithm.OrderByComparer => applicants.OrderBy(a => a, ComparerFor(role)),
                SortAlgorithm.OrderByCached when reverse => applicants.OrderByDescending(a => a, new CachedComparer<Applicant>(ComparerFor(role))),
                SortAlgorithm.OrderByCached => applicants.OrderBy(a => a, new CachedComparer<Applicant>(ComparerFor(role))),
                SortAlgorithm.QuickSortComparer when reverse => QuickSort(applicants, ComparerFor(role)).Reverse(),
                SortAlgorithm.QuickSortComparer => QuickSort(applicants, ComparerFor(role)),
                SortAlgorithm.QuickSortCached when reverse => QuickSort(applicants, new CachedComparer<Applicant>(ComparerFor(role))).Reverse(),
                SortAlgorithm.QuickSortCached => QuickSort(applicants, new CachedComparer<Applicant>(ComparerFor(role))),
                SortAlgorithm.DisagreementSort when reverse => new DisagreementSort<Applicant>(ComparerFor(role)).Sort(applicants).Reverse(),
                SortAlgorithm.DisagreementSort => new DisagreementSort<Applicant>(ComparerFor(role)).Sort(applicants),
                _ => throw new NotImplementedException($"Enum not implemented: {SortAlgorithm}")
            };

        private IEnumerable<T> QuickSort<T>(IEnumerable<T> items, IComparer<T> comparer)
        {
            var list = new List<T>(items);
            list.Sort(comparer);
            return list;
        }
        #endregion
    }
}
