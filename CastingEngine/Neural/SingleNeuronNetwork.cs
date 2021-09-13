using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    public class SingleNeuronNetwork
    {
        IActivationFunction activation = new Sigmoid();
        ILossFunction loss = new MeanSquaredError();
        public Neuron Neuron = new Neuron
        {
            Inputs = new[]
            {
                new InputFeature { Weight = 0.1 },
                new InputFeature { Weight = 0.2 }
            },
            Bias = 0.3
        };
        double learningRate = 0.05;

        public void Train(double[] inputs, double expected_output)
        {
            // Calculation
            var out_o = Predict(inputs);
            // Back propogation (stochastic gradient descent)
            var dloss_douto = loss.Derivative(out_o, expected_output);
            var douto_dino = activation.Derivative(out_o);
            var dloss_dino = dloss_douto * douto_dino;
            for (var i = 0; i < inputs.Length; i++)
            {
                var dino_dweight = inputs[i]; // because weighted sum: dino = i0*w0 + i1*w1 + bias
                var dloss_dweight = dloss_dino * dino_dweight;
                Neuron.Inputs[i].Weight -= learningRate * dloss_dweight;
            }
            var dino_dbias = 1; // because weighted sum: dino = i0*w0 + i1*w1 + bias
            Neuron.Bias -= learningRate * dloss_dino * dino_dbias;
        }

        public double Predict(double[] inputs)
        {
            var in_o = Neuron.Calculate(inputs);
            var out_o = activation.Calculate(in_o);
            return out_o;
        }

        public override string ToString() => $"Output: {Neuron}";
    }
}
