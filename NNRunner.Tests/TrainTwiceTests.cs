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
            var description = SimpleDescriptionBuilder.GetDescription(3, new[] {3, 3, 1});
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
            var tests = new[]
            {
                Tuple.Create(new[] {1f, 0f, 0f}, new[] {1f}),
                Tuple.Create(new[] {0f, 0f, 0f}, new[] {0f}),
                Tuple.Create(new[] {1f, 1f, 0f}, new[] {1f}),
                Tuple.Create(new[] {1f, 0f, 1f}, new[] {1f}),
                Tuple.Create(new[] {1f, 1f, 1f}, new[] {1f}),
                Tuple.Create(new[] {0f, 1f, 0f}, new[] {0f}),
                Tuple.Create(new[] {0f, 1f, 1f}, new[] {0f}),
                Tuple.Create(new[] {0, 0f, 1f}, new[] {0f})
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

        [Test]
        public void FullTrainToDescAndBack()
        {
            var tests = new[]
            {
                Tuple.Create(new[] {1f, 0f, 0f}, new[] {1f}),
                Tuple.Create(new[] {0f, 0f, 0f}, new[] {0f}),
                Tuple.Create(new[] {1f, 1f, 0f}, new[] {1f}),
                Tuple.Create(new[] {1f, 0f, 1f}, new[] {1f}),
                Tuple.Create(new[] {1f, 1f, 1f}, new[] {1f}),
                Tuple.Create(new[] {0f, 1f, 0f}, new[] {0f}),
                Tuple.Create(new[] {0f, 1f, 1f}, new[] {0f}),
                Tuple.Create(new[] {0, 0f, 1f}, new[] {0f})
            };

            var description = SimpleDescriptionBuilder.GetDescription(3, new[] {3, 1});
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

            var desc2 = net.Description;
            var net2 = Net.FromDescription(desc2);

            var eval1 = net.GetEvaluationFunction();
            var eval2 = net2.GetEvaluationFunction();

            var avgError1 = 0d;
            var avgError2 = 0d;
            foreach (var test in tests)
            {
                var result1 = eval1(test.Item1);
                var result2 = eval2(test.Item1);
                avgError1 += Math.Pow(result1[0] - test.Item2[0], 2);
                avgError2 += Math.Pow(result2[0] - test.Item2[0], 2);
            }
            avgError2 /= tests.Length;

            Assert.IsTrue(avgError2 <= firstError);

            Func<float[], float[]> a = ((float[] inputs) =>
            {
                var in0 = inputs[0];
                var in1 = inputs[1];
                var in2 = inputs[2];
                var agg0 = (in0 * -2.458647) + (in1 * -0.07651551) + (in2 * -0.0518001) + -0.3729805;
                var out0 = Math.Log(1 + Math.Exp(agg0));
                var agg1 = (in0 * -2.523692) + (in1 * -0.05195593) + (in2 * -0.03320933) + -0.3351826;
                var out1 = Math.Log(1 + Math.Exp(agg1));
                var agg2 = (in0 * -2.458876) + (in1 * -0.07796045) + (in2 * -0.05187172) + -0.3720873;
                var out2 = Math.Log(1 + Math.Exp(agg2));
                var agg3 = (out0 * -0.8511373) + (out1 * -0.9345574) + (out2 * -0.8509701) + 4.971978;
                var out3 = Math.Log(1 + Math.Exp(agg3));
                return new float[] {(float) out3};
            });

        }
    }
}