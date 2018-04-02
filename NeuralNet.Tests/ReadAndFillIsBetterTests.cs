using System;
using System.Collections.Generic;
using System.Text;
using NerualNet.Builder;
using NerualNet.Logic;
using NUnit.Framework;

namespace NeuralNet.Tests
{
    class ReadAndFillIsBetterTests
    {
        [Test]
        public void TestReadAndFillDoesntChangeError()
        {
            var tests = new[]
            {
                new[] {new[] {1f, 1f}, new[] {-1f}},
                new[] {new[] {1f, -1f}, new[] {1f}},
                new[] {new[] {-1f, 1f}, new[] {1f}},
                new[] {new[] {-1f, -1f}, new[] {-1f}}
            };

            var builder = new LayerBuilder();
            var desc = builder.BuildDescription(2, new[]
            {
                new LayerBuilder.LayerSpec(2, "sum", "tanh"),
                new LayerBuilder.LayerSpec(1, "sum", "tanh")
            });

            var net = Net.FromDescription(desc);
            var train = net.GetTrainingFunction();

            var weights = new float[net.NumberOfWeights];
            var deltas = new float[net.NumberOfWeights];

            net.FillWeights(weights);

            var loss = 0f;
            for (var i = 0; i < 15001; i++)
            {
                loss = 0;
                foreach (var test in tests)
                {
                    loss += train(test[0], test[1], weights, deltas);
                    for (var j = 0; j < weights.Length; j++)
                    {
                        weights[j] -= deltas[j] / 2f;
                    }
                }
                if (i % 300 == 0) Console.WriteLine($"{i}, {loss}");
            }
            loss = 0;
            foreach (var test in tests)
            {
                loss += train(test[0], test[1], weights, deltas);
            }
            Console.WriteLine(loss);

            Console.WriteLine(string.Join(", ", weights));
            net.ReadWeights(weights);

            var newWeights = new float[net.NumberOfWeights];
            net.FillWeights(newWeights);
            Console.WriteLine(string.Join(", ", newWeights));

            for (var i = 0; i < weights.Length; i++)
            {
                Assert.AreEqual(weights[i], newWeights[i]);
            }

            var newLoss = 0f;
            foreach (var test in tests)
            {
                newLoss += train(test[0], test[1], newWeights, new float[net.NumberOfWeights]);
            }

            Console.WriteLine(newLoss);
            Assert.AreEqual(loss, newLoss);
        }
    }
}
