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
    /// <summary>
    /// A concrete approach for learning the user's casting choices, by training the Neural Network one role at a time,
    /// when it is selected.
    /// </summary>
    public class RoleLearningAllocationEngine : SimpleNeuralAllocationEngine
    {
        public RoleLearningAllocationEngine(IAuditionEngine audition_engine, AlternativeCast[] alternative_casts, ShowRoot show_root, Requirement[] requirements, UserConfirmation confirm)
            : base(audition_engine, alternative_casts, show_root, requirements, confirm)
        { }

        protected override void AddTrainingPairs(Dictionary<double[], double[]> pairs, Role role)
        {
            TrainModel(pairs); // always train immediately
            PropogateChangesToShowModel(role.Requirements.Contains);
        }

        /// <summary>Empty implementation because training is always processed immediately</summary>
        protected override void FinaliseTraining()
        { }
    }
}
