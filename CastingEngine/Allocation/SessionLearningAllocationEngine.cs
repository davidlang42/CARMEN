using Carmen.CastingEngine.Audition;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Allocation
{
    /// <summary>
    /// A concrete approach for learning the user's casting choices, by training the Neural Network with many roles at once,
    /// at the end of a casting session.
    /// </summary>
    public class SessionLearningAllocationEngine : SimpleNeuralAllocationEngine
    {
        readonly Dictionary<double[], double[]> trainingPairs = new();

        #region Engine parameters
        /// <summary>If true, training will occur whenever <see cref="UserPickedCast(IEnumerable{Applicant}, IEnumerable{Applicant}, Role)"/> is called</summary>
        public bool TrainImmediately { get; set; } = false;

        /// <summary>If true, training data will kept to be used again in future training</summary>
        public bool StockpileTrainingData { get; set; } = true;
        #endregion

        public SessionLearningAllocationEngine(IAuditionEngine audition_engine, AlternativeCast[] alternative_casts, ShowRoot show_root, Requirement[] requirements, UserConfirmation confirm)
            : base(audition_engine, alternative_casts, show_root, requirements, confirm)
        { }

        #region Business logic
        protected override async Task AddTrainingPairs(Dictionary<double[], double[]> pairs, Role role)
        {
            foreach (var pair in pairs)
                trainingPairs.Add(pair.Key, pair.Value);
            if (!TrainImmediately)
                return; // do it later
            await FinaliseTraining();
        }

        protected override async Task FinaliseTraining()
        {
            if (!trainingPairs.Any())
                return; // nothing to do
            await TrainModel(trainingPairs);
            if (!StockpileTrainingData)
                trainingPairs.Clear();
            PropogateChangesToShowModel(r => true);
        }
        #endregion
    }
}
