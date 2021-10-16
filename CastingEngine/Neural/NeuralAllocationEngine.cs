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
    public abstract class NeuralAllocationEngine : WeightedAverageEngine
    {
        protected readonly IOverallWeighting[] overallWeightings;
        private Dictionary<Requirement, int> overallWeightingsLookup;
        protected readonly Requirement[] suitabilityRequirements;
        private Dictionary<Requirement, int> suitabilityRequirementsLookup;
        protected readonly ICriteriaRequirement[] existingRoleRequirements;
        private Dictionary<Requirement, int> existingRoleRequirementsLookup;
        protected readonly int nInputs;
        protected readonly UserConfirmation confirm;

        protected abstract INeuralNetwork Model { get; }

        public Montage? LastTrainingMontage { get; set; } = null;

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
        public virtual SortAlgorithm SortAlgorithm { get; set; } = SortAlgorithm.OrderBySuitability;
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
            overallWeightingsLookup = ArrayLookup(0, overallWeightings);
            suitabilityRequirements = suitability_requirements;
            suitabilityRequirementsLookup = ArrayLookup(overallWeightings.Length, suitabilityRequirements);
            existingRoleRequirements = existing_role_requirements;
            existingRoleRequirementsLookup = ArrayLookup(overallWeightings.Length + suitabilityRequirements.Length, existingRoleRequirements);
            this.confirm = confirm;
            nInputs = (overallWeightings.Length + suitabilityRequirements.Length + existingRoleRequirements.Length) * 2;
            if (overallWeightings.Length + suitabilityRequirements.Length < 2 && !ConfirmEngineCantLearn())
                throw new ApplicationException("Not enough weights are enabled for the NeuralAllocationEngine to learn from."
                    + "\nPlease enable more Suitability and Overall weights on Requirements on the Configuring Show page."
                    + "\nThis error can be avoided by choosing a non-learning AllocationEngine in the Advanced settings."); //LATER make sure that when this is thrown, the user is taken cleanly back to the main menu
        }

        private static Dictionary<Requirement, int> ArrayLookup<T>(int offset, T[] array)
        {
            Dictionary<Requirement, int> dict = new();
            for (var i = 0; i < array.Length; i++)
                if (array[i] is Requirement key)
                    dict.Add(key, i + offset);
            return dict;
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
            foreach (var (picked, not_picked) in ComparablePairs(applicants_picked, not_picked_array))
            {
                training_pairs.Add(InputValues(picked, not_picked, role), new[] { 1.0 });
                training_pairs.Add(InputValues(not_picked, picked, role), new[] { 0.0 });
            }
            if (!training_pairs.Any())
                return; // nothing to do
            // Process training data
            AddTrainingPairs(training_pairs, role);
        }

        /// <summary>Finds pairs of good and bad applicants with matching cast groups</summary>
        public static IEnumerable<(Applicant good, Applicant bad)> ComparablePairs(IEnumerable<Applicant> good_applicants, IEnumerable<Applicant> bad_applicants)
        {
            var good_by_group = good_applicants.GroupBy(a => a.CastGroup).ToDictionary(g => g.Key!, g => g.ToArray());
            var bad_by_group = bad_applicants.GroupBy(a => a.CastGroup).ToDictionary(g => g.Key!, g => g.ToArray());
            foreach (var cg in good_by_group.Keys)
                foreach (var good in good_by_group[cg])
                    if (bad_by_group.TryGetValue(cg, out var bad_of_this_group))
                        foreach (var bad in bad_of_this_group)
                            yield return (good, bad);
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
            double? overall_a = null;
            double? overall_b = null;
            for (var i = 0; i < overallWeightings.Length; i++)
                if (overallWeightings[i] is not Requirement)
                {
                    values[i] = overall_a ??= ApplicantEngine.OverallSuitability(a);
                    values[i + offset] = overall_b ??= ApplicantEngine.OverallSuitability(b);
                }
            foreach (var requirement in role.Requirements)
            {
                if (overallWeightingsLookup.TryGetValue(requirement, out int overall_index))
                {
                    values[overall_index] = overall_a ??= ApplicantEngine.OverallSuitability(a);
                    values[overall_index + offset] = overall_b ??= ApplicantEngine.OverallSuitability(b);
                }
                if (suitabilityRequirementsLookup.TryGetValue(requirement, out var suitability_index))
                {
                    values[suitability_index] = ApplicantEngine.SuitabilityOf(a, requirement);
                    values[suitability_index + offset] = ApplicantEngine.SuitabilityOf(b, requirement);
                }
                if (existingRoleRequirementsLookup.TryGetValue(requirement, out var existing_role_index))
                {
                    values[existing_role_index] = CountRoles(a, ((ICriteriaRequirement)requirement).Criteria, role);
                    values[existing_role_index + offset] = CountRoles(b, ((ICriteriaRequirement)requirement).Criteria, role);
                }
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
            LastTrainingMontage = trainer.Train(pairs.Keys, pairs.Values);
        }

        protected virtual double CalculateLearningRate() => NeuralLearningRate;
        #endregion

        #region Applicant comparison
        public override int Compare(Applicant a, Applicant b, Role for_role)
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
        protected override List<Applicant> InPreferredOrder(IEnumerable<Applicant> applicants, Role role, bool reverse = false)
        {
            return SortAlgorithm switch
            {
                SortAlgorithm.OrderBySuitability => base.InPreferredOrder(applicants, role, reverse),
                SortAlgorithm.OrderByCached when reverse => applicants.OrderByDescending(a => a, new CachedComparer<Applicant>(ComparerFor(role))).ToList(),
                SortAlgorithm.OrderByCached => applicants.OrderBy(a => a, new CachedComparer<Applicant>(ComparerFor(role))).ToList(),
                SortAlgorithm.QuickSortCached when reverse => QuickSort(applicants, new CachedComparer<Applicant>(ComparerFor(role))).AsEnumerable().Reverse().ToList(),
                SortAlgorithm.QuickSortCached => QuickSort(applicants, new CachedComparer<Applicant>(ComparerFor(role))).ToList(),
                SortAlgorithm.DisagreementSort when reverse => new DisagreementSort<Applicant>(ComparerFor(role)).Sort(applicants).Reverse().ToList(),
                SortAlgorithm.DisagreementSort => new DisagreementSort<Applicant>(ComparerFor(role)).Sort(applicants).ToList(),
                _ => throw new NotImplementedException($"Enum not implemented: {SortAlgorithm}")
            };
        }

        private List<T> QuickSort<T>(IEnumerable<T> items, IComparer<T> comparer)
        {
            var list = new List<T>(items);
            list.Sort(comparer);
            return list;
        }
        #endregion
    }
}
