using Carmen.CastingEngine.Neural;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Neural
{
    class FeedforwardNetworkTests
    {
        const double LOSS_THRESHOLD = 0.001;

        [Test]
        public void Simple()
        {
            var network = new FeedforwardNetwork(2, 1, 2, 2);
            var training_data = new Dictionary<double[], double[]>() // OR, AND
            {
                { new double[] { 0, 0 }, new double[] { 0, 0 } },
                { new double[] { 1, 0 }, new double[] { 1, 0 } },
                { new double[] { 0, 1 }, new double[] { 1, 0 } },
                { new double[] { 1, 1 }, new double[] { 1, 1 } },
            };
            Console.WriteLine(network);
            var repeat = 0;
            for (var success = false; !success; repeat++)
            {
                success = true;
                foreach (var (inputs, output) in training_data)
                    success &= network.Train(inputs, output) < LOSS_THRESHOLD;
                if (repeat % 1000 == 0)
                    Console.WriteLine(network);
            }
            Console.WriteLine($"Achieved <{LOSS_THRESHOLD} loss after {repeat} iterations");
            foreach (var (inputs, expected_results) in training_data)
            {
                var predictions = network.Predict(inputs);
                for (var i = 0; i < expected_results.Length; i++)
                    predictions[i].Should().BeApproximately(expected_results[i], 0.1);
                Console.WriteLine($"Predict: [{string.Join(", ", inputs)}] => [{string.Join(", ", predictions)}]");
            }
            repeat.Should().BeLessThan(10000);
        }

        [TestCase(2, 1, 4, 1)]
        [TestCase(2, 1, 8, 1)]
        [TestCase(2, 2, 4, 1)]
        [TestCase(2, 1, 16, 1)]
        [TestCase(2, 2, 8, 1)]
        [Test]
        public void XOR_Succeeds(int n_inputs, int n_hidden_layers, int n_neurons_per_hidden_layer, int n_outputs)
        {
            var network = new FeedforwardNetwork(n_inputs, n_hidden_layers, n_neurons_per_hidden_layer, n_outputs);
            var training_data = new Dictionary<double[], double[]>()
            {
                {new double[] {0, 0}, new double[] { 0 } },
                {new double[] {0, 1}, new double[] { 1 } },
                {new double[] {1, 0}, new double[] { 1 } },
                {new double[] {1, 1}, new double[] { 0 } },
            };
            Console.WriteLine(network);
            var repeat = 0;
            for (var success = false; !success; repeat++)
            {
                success = true;
                foreach (var (inputs, output) in training_data)
                    success &= network.Train(inputs, output) < LOSS_THRESHOLD;
                if (repeat % 1000 == 0)
                    Console.WriteLine(network);
            }
            Console.WriteLine($"Achieved <{LOSS_THRESHOLD} loss after {repeat} iterations");
            foreach (var (inputs, expected_result) in training_data)
            {
                var prediction = network.Predict(inputs);
                prediction[0].Should().BeApproximately(expected_result[0], 0.1);
                Console.WriteLine($"Predict: [{string.Join(", ", inputs)}] => {prediction[0]}");
            }
            repeat.Should().BeLessThan(100000); // often needs more than 10000 to stabilise
        }
    }
}
