using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    public class SingleLayerNetwork
    {
        ILossFunction loss = new MeanSquaredError();
        public Layer layer;
        double learningRate = 0.05;

        public SingleLayerNetwork(int n_inputs, int n_outputs)
        {
            layer = new Layer(n_inputs, n_outputs);
        }

        public void Train(double[] inputs, double[] expected_outputs)
        {
            // Calculation
            var out_o = Predict(inputs);
            // Back propogation (stochastic gradient descent)
            var dloss_douto = new double[layer.Neurons.Length];
            for (var n = 0; n < layer.Neurons.Length; n++)
                dloss_douto[n] = loss.Derivative(out_o[n], expected_outputs[n]);//TODO make this a single array call
            layer.Train(inputs, out_o, dloss_douto, learningRate);
        }

        public double[] Predict(double[] inputs) => layer.Predict(inputs);

        public override string ToString() => $"Outputs: {layer}";
    }
}