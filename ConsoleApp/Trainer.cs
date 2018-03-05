using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleApp
{
    public class Trainer
    {
        private readonly IEnumerable<Tuple<float[], float[]>> _testData;
        private readonly Net _net;
        private readonly Random _random = new Random();

        public Trainer(IEnumerable<Tuple<float[], float[]>> testData, Net net)
        {
            _testData = testData;
            _net = net;
        }

        public void Train(float learnFactor, float inertia, float desiredError, int maxRuns, bool initializeWeights = false)
        {
            var weights = new float[_net.NumberOfWeights];
            _net.FillWeights(weights);

            if (initializeWeights)
            {
                for (var i = 0; i < weights.Length; i++)
                {
                    weights[i] = (float)(_random.NextDouble() * .2f) - .1f;
                }
            }

            var getDeltas = _net.GetTrainingFunction();
            var speeds = new float[_net.NumberOfWeights];

            var minDeltas = new float[_net.NumberOfWeights + 1];
            var minWeights = new float[_net.NumberOfWeights];
            float minError = float.MaxValue;
            float avgError = float.MaxValue;
            int runCount = 0;
            while (runCount < maxRuns && avgError > desiredError && learnFactor > 0.005f)
            {
                runCount += 1;

                var deltas = _testData.Select(test => getDeltas(test.Item1, test.Item2, weights));
                var avgDeltas = new float[_net.NumberOfWeights + 1];
                foreach (var delta in deltas)
                {
                    for (var i = 0; i < avgDeltas.Length; i++)
                    {
                        avgDeltas[i] += delta[i];
                    }
                }
                for (var i = 0; i < avgDeltas.Length; i++) avgDeltas[i] /= deltas.Count();
                avgError = avgDeltas.Last();

                if (runCount % 500 == 0) Console.WriteLine($"After {runCount} runs, error is {avgError}");

                if (avgError > 1.10f * minError || float.IsNaN(avgError))
                {
                    inertia *= .5f;
                    learnFactor *= .9f;
                    Console.WriteLine($"Resetting to last run, new learnFactor = {learnFactor}, new inertia = {inertia}");
                    speeds = new float[_net.NumberOfWeights];
                    Array.Copy(minWeights, weights, weights.Length);
                    avgDeltas = minDeltas;
                    avgError = minError;
                }
                else if (avgError < minError)
                {
                    minError = avgError;
                    minDeltas = avgDeltas;
                    Array.Copy(weights, minWeights, minWeights.Length);
                }

                for (var i = 0; i < weights.Length; i++)
                {
                    speeds[i] = (inertia * speeds[i]) + (learnFactor * avgDeltas[i]);
                    weights[i] -= speeds[i];
                    //if (weights[i] < -3) weights[i] = -3f;
                    //else if (weights[i] > 3) weights[i] = 3f;
                }
            }

            Array.Copy(minWeights, weights, weights.Length);
            _net.ReadWeights(weights);
        }
    }
}
