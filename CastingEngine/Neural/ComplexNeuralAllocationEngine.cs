using Carmen.CastingEngine.Base;
using Carmen.CastingEngine.Heuristic;
using Carmen.CastingEngine.Neural.Internal;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Carmen.CastingEngine.Neural
{
    /// <summary>
    /// A concrete approach for learning the user's casting choices, using a Feed-forward Neural Network with customisable complexity.
    /// This complexity requires storage of the neural network weights outside the ShowModel.
    /// </summary>
    public class ComplexNeuralAllocationEngine : NeuralAllocationEngine
    {
        readonly FeedforwardNetwork model;
        readonly Dictionary<double[], double[]> trainingPairs = new();

        protected override INeuralNetwork Model => model;

        //LATER allow users to change these parameters
        #region Engine parameters
        /// <summary>If true, training will occur whenever <see cref="UserPickedCast(IEnumerable{Applicant}, IEnumerable{Applicant}, Role)"/> is called</summary>
        public bool TrainImmediately { get; set; } = false;

        /// <summary>If true, training data will kept to be used again in future training</summary>
        public bool StockpileTrainingData { get; set; } = true;

        /// <summary>If false, the model will only be used for predictions, but not updated</summary>
        public bool AllowTraining { get; set; } = true;

        public string ModelFileName { get; private init; } //LATER make this a setting, editable by user
        #endregion

        public ComplexNeuralAllocationEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, ShowRoot show_root, Requirement[] requirements, UserConfirmation confirm, string model_file_name)
            : base(applicant_engine, alternative_casts, show_root, show_root.Yield().ToArray(), requirements, requirements.OfType<ICriteriaRequirement>().ToArray(), confirm)
        {
            ModelFileName = model_file_name;
            model = LoadModelFromDisk(nInputs, model_file_name);
        }

        #region Business logic
        public override void ExportChanges()
        {
            base.ExportChanges();
            if (AllowTraining)
                SaveModelToDisk(ModelFileName, model);
        }

        protected override void AddTrainingPairs(Dictionary<double[], double[]> pairs, Role role)
        {
            if (!AllowTraining)
                return; // nothing to do
            foreach (var pair in pairs)
                trainingPairs.Add(pair.Key, pair.Value);
            if (!TrainImmediately)
                return; // do it later
            FinaliseTraining();
        }

        protected override void FinaliseTraining()
        {
            if (!trainingPairs.Any())
                return; // nothing to do
            TrainModel(trainingPairs);
            if (!StockpileTrainingData)
                trainingPairs.Clear();
        }

        private FeedforwardNetwork LoadModelFromDisk(int n_inputs, string file_name)
        {
            //LATER handle changes in requirements
            // - store associated to requirement names, if names change it counts as a new requirement
            // - any new requirements get initialised with random weights of the correct polarity
            // - existing requirements may be re-ordered, but this just involves updating the first layer weights
            if (!string.IsNullOrEmpty(file_name) && File.Exists(file_name))
            {
                var reader = new XmlSerializer(typeof(FeedforwardNetwork));
                try
                {
                    using var file = new StreamReader(file_name);
                    if (reader.Deserialize(file) is FeedforwardNetwork model && model.InputCount == n_inputs)
                        return model;
                }
                catch
                {
                    //LATER log exception or otherwise tell user, there are many cases that can get here: file access issue, corrupt/invalid file format, file contains model with wrong number of inputs
                }
            }
            //TODO allow parameters to be configured (layers, neurons, hidden activation functions) -- these will need to be passed into the constructor
            var new_model = new FeedforwardNetwork(n_inputs, 2, n_inputs, 1); // sigmoid output is between 0 and 1, crossing at 0.5
            foreach (var neuron in new_model.Layers.First().Neurons)
                FlipPolarities(neuron);
            return new_model;
        }

        private static void SaveModelToDisk(string file_name, FeedforwardNetwork model)
        {
            var writer = new XmlSerializer(typeof(FeedforwardNetwork));
            //LATER handle exceptions
            using var file = new StreamWriter(file_name);
            writer.Serialize(file, model);
        }
        #endregion

        #region Applicant comparison
        //TODO how will suitability be calculated? or do we just adjust ordering to used Disagreement sort instead of suitability?
        public override double SuitabilityOf(Applicant applicant, Role role) => throw new NotImplementedException();
        #endregion

        #region Helper methods
        private void FlipPolarities(Neuron neuron)
        {
            neuron.Bias = 0;
            var offset = nInputs / 2;
            var i = 0;
            foreach (var ow in overallWeightings)
            {
                neuron.Weights[i + offset] *= -1;
                i++;
            }
            foreach (var requirement in suitabilityRequirements)
            {
                neuron.Weights[i + offset] *= -1;
                i++;
            }
            foreach (var requirement in existingRoleRequirements)
            {
                neuron.Weights[i] *= -1;
                i++;
            }
        }
        #endregion
    }
}
