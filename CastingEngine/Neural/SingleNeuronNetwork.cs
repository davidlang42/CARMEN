using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    public class SingleNeuronNetwork
    {
        IScalarActivationFunction activation = new Sigmoid();
        ILossFunction loss = new MeanSquaredError();
        public Neuron Neuron;
        double learningRate = 0.05;

        public SingleNeuronNetwork(int n_inputs)
        {
            Neuron = new Neuron(n_inputs);
        }

        public void Train(double[] inputs, double expected_output)
        {
            // Calculation
            var out_o = Predict(inputs);
            // Back propogation (stochastic gradient descent)
            var dloss_douto = loss.Derivative(new[] { out_o }, new[] { expected_output })[0];
            var douto_dino = activation.Derivative(out_o);
            var dloss_dino = dloss_douto * douto_dino;
            for (var i = 0; i < inputs.Length; i++)
            {
                var dino_dweight = inputs[i]; // because weighted sum: dino = i0*w0 + i1*w1 + bias
                var dloss_dweight = dloss_dino * dino_dweight;
                Neuron.Weights[i] -= learningRate * dloss_dweight;
            }
            var dino_dbias = 1; // because weighted sum: dino = i0*w0 + i1*w1 + bias
            Neuron.Bias -= learningRate * dloss_dino * dino_dbias;
        }

        public double Predict(double[] inputs)
        {
            var in_o = Neuron.WeightedSum(inputs);
            var out_o = activation.Calculate(in_o);
            return out_o;
        }

        public override string ToString() => $"Output: {Neuron}";
    }
}
