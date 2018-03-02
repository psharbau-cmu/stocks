using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp
{
    public class Trainer
    {
        private readonly IEnumerable<Tuple<float[], float[]>> _testData;
        private readonly Net _net;

        private float[] _weights;

        public Trainer(IEnumerable<Tuple<float[], float[]>> testData, Net net)
        {
            _testData = testData;
            _net = net;
        }

        public void Train(float learnFactor, float inertia, float desiredError, int maxRuns)
        {
            var weights = new float[_net.NumberOfWeights];
            _net.FillWeights(weights);

            var getDeltas = _net.GetTrainingFunction();
            var speeds = new float[_net.NumberOfWeights];

            float avgError = float.MaxValue;
            int runCount = 0;
            while (runCount < maxRuns && avgError > desiredError)
            {
                runCount += 1;
                var avgCount = 1;
                avgError = 0;

                foreach (var test in _testData)
                {
                    var deltas = getDeltas(test.Item1, test.Item2, weights);
                    var error = deltas[deltas.Length - 1];
                    avgError += (error - avgError) / avgCount;
                    avgCount += 1;

                    for (var i = 0; i < weights.Length; i++)
                    {
                        speeds[i] = (inertia * speeds[i]) + (learnFactor * deltas[i]);
                        weights[i] += speeds[i];
                    }
                }

                Console.WriteLine($"After {runCount} runs, error is {avgError}");
            }

            _net.ReadWeights(weights);
        }
    }
}
