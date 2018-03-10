using System;
using System.Collections.Generic;
using NNRunner.NeuralNet;
using NUnit.Framework;
using System.Threading;

namespace Tests
{
    public class Tests
    {
        [Test]
        public void ReadAndFillAreEqual()
        {
            var description = SimpleDescriptionBuilder.GetDescription(3, new[] { 3, 3, 1 });
            var net = Net.FromDescription(description);

            var weights = new float[net.NumberOfWeights];
            for (var i = 0; i < weights.Length; i++)
            {
                weights[i] = (float) i;
            }

            net.ReadWeights(weights);

            var newWeights = new float[net.NumberOfWeights];
            net.FillWeights(newWeights);

            for (var i = 0; i < weights.Length; i++)
            {
                Assert.AreEqual(weights[i], newWeights[i]);
            }
        }

        [Test]
        public void TrainingTwiceStartsWhereFirstCycleEnded()
        {
            var tests = new []
            {
                Tuple.Create(new[] {1f, 0f, 0f}, new[] {1f}),
                Tuple.Create(new [] {0f, 0f, 0f}, new []{0f}),
                Tuple.Create(new [] {1f, 1f, 0f}, new []{1f}),
                Tuple.Create(new [] {1f, 0f, 1f}, new []{1f}),
                Tuple.Create(new [] {1f, 1f, 1f}, new []{1f}),
                Tuple.Create(new [] {0f, 1f, 0f}, new []{0f}),
                Tuple.Create(new [] {0f, 1f, 1f}, new []{0f}),
                Tuple.Create(new [] {0, 0f, 1f}, new []{0f})
            };

            var description = SimpleDescriptionBuilder.GetDescription(3, new[] {3, 3, 1});
            var net = Net.FromDescription(description);
            WeightFiller.FillWeights(net, 0.001f);

            var source = new CancellationTokenSource();
            var token = source.Token;

            var trainer = new Trainer(tests, net);

            var firstError = trainer.Train(
                learnFactor: 0.5f,
                inertia: 0.2f,
                desiredError: 0.0001f,
                maxRuns: 50,
                progress: j => { },
                cancel: token);

            var secondError = float.MaxValue;
                
            trainer.Train(
                learnFactor: 0.5f,
                inertia: 0.2f,
                desiredError: 0.0001f,
                maxRuns: 2,
                progress: j => { secondError = j.AvgError; },
                cancel: token);

            Assert.IsTrue(secondError < firstError);
        }
    }
}