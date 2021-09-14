using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    /// <summary>
    /// An artificial neural network which takes input features from an object of type <typeparamref name="T"/>
    /// and predicts output features as an object of type <typeparamref name="U"/>.
    /// </summary>
    public class NeuralNetwork<T, U>
        where T : struct, IInputFeatureSet
        where U: struct, IOutputFeatureSet
    {
        FeedforwardNetwork network;

        public NeuralNetwork(int n_hidden_layers, int n_neurons_per_hidden_layer,
            IVectorActivationFunction? hidden_layer_activation = null,
            IVectorActivationFunction? output_layer_activation = null)
        {
            network = new FeedforwardNetwork(IInputFeatureSet.GetSize<T>(), n_hidden_layers, n_neurons_per_hidden_layer,
                IInputFeatureSet.GetSize<U>(), hidden_layer_activation, output_layer_activation);
        }

        /// <summary>Train the model with a single input and expected output.
        /// Returns the total loss prior to back propogation.</summary>
        public double Train(T input, U expected_output) => network.Train(input.GetValues(), expected_output.GetValues());

        public U Predict(T input)
        {
            var result = new U();
            result.SetValues(network.Predict(input.GetValues()));
            return result;
        }

        public override string ToString() => network.ToString();
    }
}
