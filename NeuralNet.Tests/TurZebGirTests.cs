using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NerualNet.Logic;
using NerualNet.Training;
using NeuralNet.Builder;
using NUnit.Framework;

namespace NeuralNet.Tests
{
    class TurZebGirTests
    {
        [Test]
        public void TurtleZebraGiraffe()
        {
            var tests = new[]
            {
                Tuple.Create(new[] {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f}, new[] {0f, 0f, 1f}),
                Tuple.Create(new[] {1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f}, new[] {1f, 0f, 0f}),
                Tuple.Create(new[] {1f, 1f, 1f, 1f, -1f, -1f, -1f, -1f, -1f, -1f}, new[] {0f, 1f, 0f})
            };
            var description = SimpleDescriptionBuilder.GetDescription(10, new[] {5, 5, 5, 3});
            foreach (var node in description.Nodes.Where(node => description.Outputs.Contains(node.NodeId)))
            {
                node.Processor = "sigmoid";
            }
            var net = Net.FromDescription(description);
            WeightFiller.FillWeights(net, .005f);

            var trainer = new SimpleTrainer();
            var error = trainer.Train(
                net: net,
                tests: tests,
                desiredError: 0.01f,
                maxEpochs: 20000,
                learningRate: .5f);

            Assert.IsTrue(error < .01f);

            var eval = net.GetEvaluationFunction();
            foreach (var test in tests)
            {
                var output = eval(test.Item1);
                Console.WriteLine(string.Join(",", output));
            }
        }
    }
}
