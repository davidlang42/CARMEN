using Carmen.CastingEngine.Neural;
using Carmen.CastingEngine.SAT;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests.SAT;

namespace UnitTests.Neural
{
    class NeuralNetworkTests
    {
        [Test]
        public void Linear_Normalized()
        {
            var model = new SingleLayerPerceptron(4, 1, ActivationFunctionChoice.LeakyReLu)
            {
                LearningRate = 0.05
            };
            TestFunction(model, v => 40 * v[2] + 30 * v[1] + 20 * v[0] + 10,
                n_data_points: 100, min: 0, max: 1);
        }

        [Test]
        public void Linear_0To100()
        {
            var model = new SingleLayerPerceptron(4, 1, ActivationFunctionChoice.LeakyReLu)
            {
                LearningRate = 0.0001
            };
            TestFunction(model, v => 40 * v[2] / 100 + 30 * v[1] / 100 + 20 * v[0] / 100 + 10,
                n_data_points: 100, min: 0, max: 100);
        }

        public void TestFunction(INeuralNetwork model, Func<double[], double> func, int n_data_points, double min, double max)
            => TestFunction(model, inputs => new[] { func(inputs) }, n_data_points, min, max);

        public void TestFunction(INeuralNetwork model, Func<double[], double[]> func, int n_data_points, double min, double max)
        {
            var trainer = new ModelTrainer(model)
            {
                LossThreshold = 0.005,
            };
            var random = new Random(1);
            var training_inputs = new double[n_data_points][];
            var training_outputs = new double[n_data_points][];
            for (var i = 0; i < n_data_points; i++)
            {
                training_inputs[i] = new double[model.InputCount];
                for (var v = 0; v < model.InputCount; v++)
                    training_inputs[i][v] = random.NextDouble() * (max - min) + min;
                training_outputs[i] = func(training_inputs[i]);
            }
            var m = trainer.Train(training_inputs, training_outputs);
            if (m.Success)
                Console.WriteLine($"Achieved {m.FinalAverageLoss:0.000} loss after {m.Iterations} iterations of {n_data_points} data points");
            if (m.ReachedStableLoss)
                Console.WriteLine($"Loss stabilised at {m.FinalAverageLoss:0.000} loss after {m.Iterations} iterations of {n_data_points} data points");
            if (m.ReachedMaxIterations)
                Console.WriteLine($"Stopped at {m.FinalAverageLoss:0.000} loss after {m.Iterations} iterations of {n_data_points} data points");
            foreach (var description in m.Descriptions)
                Console.WriteLine(description);
            Console.WriteLine(model);
            for (var i = 0; i < n_data_points; i++)
            {
                var prediction = model.Predict(training_inputs[i]);
                Console.WriteLine($"Predict: [{string.Join(", ", training_inputs[i])}] => {string.Join(", ", prediction)} [{string.Join(", ", training_outputs[i])}]");
            }
            for (var i = 0; i < n_data_points; i++)
            {
                var prediction = model.Predict(training_inputs[i]);
                for (var v = 0; v < model.OutputCount; v++)
                    prediction[v].Should().BeApproximately(training_outputs[i][v], 0.1);
            }
            if (!m.Success)
                Assert.Fail();
        }
    }
}
