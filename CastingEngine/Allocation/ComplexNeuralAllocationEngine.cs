﻿using Carmen.CastingEngine.Audition;
using Carmen.CastingEngine.Neural;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Carmen.CastingEngine.Allocation
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

        #region Engine parameters
        /// <summary>If true, training will occur whenever <see cref="UserPickedCast(IEnumerable{Applicant}, IEnumerable{Applicant}, Role)"/> is called</summary>
        public bool TrainImmediately { get; set; } = false;

        /// <summary>If true, training data will kept to be used again in future training</summary>
        public bool StockpileTrainingData { get; set; } = true;

        /// <summary>If false, the model will only be used for predictions, but not updated</summary>
        public bool AllowTraining { get; set; } = true;

        /// <summary>The method used to save and load the neural network model</summary>
        public IDataPersistence ModelPersistence { get; set; }

        /// <summary>The number of hidden layers to be created in a new model (does not affect loaded models)</summary>
        public int NeuralHiddenLayers { get; set; } = 1;

        /// <summary>The constant number of neurons to be created in a new model layer (does not affect loaded models)
        /// NOTE: This is used in conjunction with <see cref="NeuralLayerNeuronsPerInput"/></summary>
        public int NeuralLayerNeuronsConstant { get; set; } = 0;

        /// <summary>The number of neurons per input to be created in a new model layer (does not affect loaded models)
        /// NOTE: This is used in conjunction with <see cref="NeuralLayerNeuronsConstant"/></summary>
        public double NeuralLayerNeuronsPerInput { get; set; } = 1;

        /// <summary>Determines which activation function is used for the hidden layers of a new model (does not affect loaded models)</summary>
        public ActivationFunctionChoice NeuralHiddenActivationFunction { get; set; } = ActivationFunctionChoice.ExponentialLu;

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

        public ComplexNeuralAllocationEngine(IAuditionEngine audition_engine, AlternativeCast[] alternative_casts, ShowRoot show_root, Requirement[] requirements, UserConfirmation confirm, IDataPersistence model_persistence)
            : base(audition_engine, alternative_casts, show_root, show_root.Yield().ToArray(), requirements, requirements.OfType<ICriteriaRequirement>().ToArray(), confirm)
        {
            MaxTrainingIterations = 100;
            NeuralLossFunction = LossFunctionChoice.BinaryCrossEntrophy;
            CountRolesByGeometricMean = false;
            model = new Lazy<FeedforwardNetwork>(LoadModelFromDisk);
            SortAlgorithm = SortAlgorithm.DisagreementSort;
            ModelPersistence = model_persistence;
        }

        #region Business logic
        public override async Task ExportChanges()
        {
            await base.ExportChanges();
            if (AllowTraining)
                await Task.Run(() => SaveModelToDisk());
        }

        protected override async Task AddTrainingPairs(Dictionary<double[], double[]> pairs, Role role)
        {
            if (!AllowTraining)
                return; // nothing to do
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
        }

        private FeedforwardNetwork LoadModelFromDisk()
        {
            var reader = new XmlSerializer(typeof(FeedforwardNetwork));
            try
            {
                using var stream = ModelPersistence.Load();
                if (reader.Deserialize(stream) is not FeedforwardNetwork model)
                    throw new ApplicationException("Failed to deserialise FeedforwardNetwork");
                if (model.InputCount != nInputs)
                    throw new ApplicationException($"Deserialised model contained {model.InputCount} inputs, expected {nInputs}");
                return model;
            }
            catch (Exception ex)
            {
                // file access issue, corrupt/invalid file format, file contains model with wrong number of inputs
                Log.Error(ex, nameof(LoadModelFromDisk));
                if (confirm($"Neural model failed to load. Would you like to create a new one?"))
                    return BuildNewModel();
                else
                    throw new UserException(ex, "Failed to load neural model.");
            }
        }

        private FeedforwardNetwork BuildNewModel()
        {
            var neurons_per_layer = Convert.ToInt32(Math.Ceiling(NeuralLayerNeuronsConstant + NeuralLayerNeuronsPerInput * nInputs));
            if (NeuralHiddenLayers < 1)
                throw new ApplicationException("The number of hidden layers must be at least 1");
            if (neurons_per_layer < 2)
                throw new ApplicationException("The number of neurons per layer must be at least 2");
            var new_model = new FeedforwardNetwork(nInputs, NeuralHiddenLayers, neurons_per_layer, 1, NeuralHiddenActivationFunction, ActivationFunctionChoice.Sigmoid); // sigmoid output is between 0 and 1, crossing at 0.5
            foreach (var neuron in new_model.Layers.First().Neurons)
                FlipPolarities(neuron);
            return new_model;
        }

        private void SaveModelToDisk()
        {
            if (!model.IsValueCreated)
                return; // no need to save if we haven't even loaded it
            var writer = new XmlSerializer(typeof(FeedforwardNetwork));
            try
            {
                using var stream = ModelPersistence.Save();
                writer.Serialize(stream, model.Value);
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(SaveModelToDisk));
                var temp = Path.GetTempFileName();
                if (confirm($"Neural model failed to save. Would you like to save it to {temp}?"))
                    using (var stream = new FilePersistence(temp).Save())
                        writer.Serialize(stream, model.Value);
            }
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
