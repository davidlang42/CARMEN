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
    class SingleLayerPerceptronTests
    {
        const double LOSS_THRESHOLD = 0.0025;
        const double CHANGE_THRESHOLD = 0.0000001;
        const int MAX_REPEATS = 10000;

        [Test]
        public void Simple()
        {
            var network = new SingleLayerPerceptron(2, 2);
            var training_data = new[] // OR, AND
            {
                (new double[] { 0, 0 }, new double[] { 0, 0 }),
                (new double[] { 1, 0 }, new double[] { 1, 0 }),
                (new double[] { 0, 1 }, new double[] { 1, 0 }),
                (new double[] { 1, 1 }, new double[] { 1, 1 }),
            };
            Console.WriteLine(network);
            var repeat = 0;
            var previous_loss = new double[training_data.Length];
            var success = false;
            var no_change = false;
            for (; !success && !no_change && repeat < MAX_REPEATS; repeat++)
            {
                success = true;
                no_change = true;
                for (var t = 0; t < training_data.Length; t++)
                {
                    var new_loss = network.Train(training_data[t].Item1, training_data[t].Item2);
                    no_change &= Math.Abs(new_loss - previous_loss[t]) < CHANGE_THRESHOLD;
                    previous_loss[t] = new_loss;
                    success &= new_loss < LOSS_THRESHOLD;
                }
                if (repeat % 1000 == 0)
                    Console.WriteLine(network);
            }
            if (success)
                Console.WriteLine($"Achieved <{LOSS_THRESHOLD} loss after {repeat} iterations of {training_data.Length} data points");
            if (no_change)
                Console.WriteLine($"Stopped at {previous_loss.Average():0.00} loss after {repeat} iterations of {training_data.Length} data points");
            foreach (var (inputs, expected_results) in training_data)
            {
                var predictions = network.Predict(inputs);
                for (var i=0; i < expected_results.Length; i++)
                    predictions[i].Should().BeApproximately(expected_results[i], 0.1);
                Console.WriteLine($"Predict: [{string.Join(", ", inputs)}] => [{string.Join(", ",predictions)}");
            }
            repeat.Should().BeLessThan(10000);
        }

        [Test]
        public void XOR_Fails()
        {
            var network = new SingleLayerPerceptron(2, 1);
            var training_data = new Dictionary<double[], double[]>()
            {
                {new double[] {0, 0}, new double[] { 0 } },
                {new double[] {0, 1}, new double[] { 1 } },
                {new double[] {1, 0}, new double[] { 1 } },
                {new double[] {1, 1}, new double[] { 0 } },
            };
            Console.WriteLine(network);
            for (var repeat = 0; repeat < 10000; repeat++)
            {
                foreach (var (inputs, output) in training_data)
                    network.Train(inputs, output);
                if (repeat % 1000 == 0)
                    Console.WriteLine(network);
            }
            foreach (var (inputs, expected_results) in training_data)
            {
                var predictions = network.Predict(inputs);
                for (var i = 0; i < expected_results.Length; i++)
                    predictions[i].Should().NotBeApproximately(expected_results[i], 0.1);
                Console.WriteLine($"Predict: [{string.Join(", ", inputs)}] => [{string.Join(", ", predictions)}");
            }
        }
    }
}
