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
        readonly Lazy<FeedforwardNetwork> model;
        readonly Dictionary<double[], double[]> trainingPairs = new();

        protected override INeuralNetwork Model => model.Value;

        //LATER allow users to change these parameters
        #region Engine parameters
        /// <summary>If true, training will occur whenever <see cref="UserPickedCast(IEnumerable{Applicant}, IEnumerable{Applicant}, Role)"/> is called</summary>
        public bool TrainImmediately { get; set; } = false;

        /// <summary>If true, training data will kept to be used again in future training</summary>
        public bool StockpileTrainingData { get; set; } = true;

        /// <summary>If false, the model will only be used for predictions, but not updated</summary>
        public bool AllowTraining { get; set; } = true;

        /// <summary>The file name of where the model should be loaded from (if existing) and stored</summary>
        public string ModelFileName { get; set; } = "";

        /// <summary>The number of hidden layers to be created in a new model (does not affect loaded models)</summary>
        public uint ModelHiddenLayers { get; set; } = 2; //LATER set a better default based on experimental data

        /// <summary>The constant number of neurons to be created in a new model layer (does not affect loaded models)
        /// NOTE: This is used in conjunction with <see cref="ModelLayerNeuronsPerInput"/></summary>
        public uint ModelLayerNeuronsConstant { get; set; } = 0; //LATER set a better default based on experimental data

        /// <summary>The number of neurons per input to be created in a new model layer (does not affect loaded models)
        /// NOTE: This is used in conjunction with <see cref="ModelLayerNeuronsConstant"/></summary>
        public uint ModelLayerNeuronsPerInput { get; set; } = 1; //LATER set a better default based on experimental data

        public override SortAlgorithm SortAlgorithm
        {
            get => base.SortAlgorithm;
            set
            {
                if (value == SortAlgorithm.OrderBySuitability)
                    throw new ArgumentException($"{nameof(ComplexNeuralAllocationEngine)} cannot use Sort Algorithm {value}");
                base.SortAlgorithm = value;
            }
        }
        #endregion

        public ComplexNeuralAllocationEngine(IApplicantEngine applicant_engine, AlternativeCast[] alternative_casts, ShowRoot show_root, Requirement[] requirements, UserConfirmation confirm)
            : base(applicant_engine, alternative_casts, show_root, show_root.Yield().ToArray(), requirements, requirements.OfType<ICriteriaRequirement>().ToArray(), confirm)
        {
            model = new Lazy<FeedforwardNetwork>(LoadModelFromDisk);
            SortAlgorithm = SortAlgorithm.DisagreementSort;
        }

        #region Business logic
        public override void ExportChanges()
        {
            base.ExportChanges();
            if (AllowTraining)
                SaveModelToDisk();
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

        private FeedforwardNetwork LoadModelFromDisk()
        {
            //LATER handle changes in requirements
            // - store associated to requirement names, if names change it counts as a new requirement
            // - any new requirements get initialised with random weights of the correct polarity
            // - existing requirements may be re-ordered, but this just involves updating the first layer weights
            if (!string.IsNullOrEmpty(ModelFileName) && File.Exists(ModelFileName))
            {
                var reader = new XmlSerializer(typeof(FeedforwardNetwork));
                try
                {
                    using var file = new StreamReader(ModelFileName);
                    if (reader.Deserialize(file) is FeedforwardNetwork model && model.InputCount == nInputs)
                        return model;
                }
                catch
                {
                    //LATER log exception or otherwise tell user, there are many cases that can get here: file access issue, corrupt/invalid file format, file contains model with wrong number of inputs
                }
            }
            return BuildNewModel();
        }

        private FeedforwardNetwork BuildNewModel()
        {
            var neurons_per_layer = (int)ModelLayerNeuronsConstant + (int)ModelLayerNeuronsPerInput * nInputs;
            if (ModelHiddenLayers < 1)
                throw new ApplicationException("The number of hidden layers must be at least 1");
            if (neurons_per_layer < 2)
                throw new ApplicationException("The number of neurons per layer must be at least 2");
            var new_model = new FeedforwardNetwork(nInputs, (int)ModelHiddenLayers, neurons_per_layer, 1); // sigmoid output is between 0 and 1, crossing at 0.5
            foreach (var neuron in new_model.Layers.First().Neurons)
                FlipPolarities(neuron);
            return new_model;
        }

        private void SaveModelToDisk()
        {
            if (!model.IsValueCreated)
                return; // no need to save if we haven't even loaded it
            var writer = new XmlSerializer(typeof(FeedforwardNetwork));
            //LATER handle exceptions (and check for filename not being empty) and offer to save to temp as a fallback
            using var file = new StreamWriter(ModelFileName);
            writer.Serialize(file, model.Value);
        }
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
