using NerualNet.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NeuralNet.Training
{
    public class Trainer
    {
        private readonly IEnumerable<Tuple<float[], float[]>> _testData;
        private readonly Net _net;

        public Trainer(IEnumerable<Tuple<float[], float[]>> testData, Net net)
        {
            _testData = testData;
            _net = net;
        }

        public float Train(
            float learnFactor,
            float inertia,
            float desiredError,
            int maxRuns,
            Action<TrainingJob> progress,
            CancellationToken cancel)
        {
            var testCount = _testData.Count();

            var weights = new float[_net.NumberOfWeights];
            var deltas = new float[_net.NumberOfWeights + 1];
            _net.FillWeights(weights);

            var getDeltas = _net.GetTrainingFunction();
            var speeds = new float[_net.NumberOfWeights];

            var equalityCheck = 0;
            
            var minWeights = new float[_net.NumberOfWeights];
            float minError = float.MaxValue;
            double avgError = double.MaxValue;
            float lastError = float.MaxValue;
            int runCount = 0;
            while (runCount < maxRuns && avgError > desiredError && learnFactor > 0.005f && !cancel.IsCancellationRequested)
            {
                runCount += 1;

                avgError = 0;
                foreach (var test in _testData)
                {
                    getDeltas(test.Item1, test.Item2, weights, deltas);
                    avgError += deltas.Last();

                    for (var i = 0; i < weights.Length; i++)
                    {
                        speeds[i] = (inertia * speeds[i]) + (learnFactor * deltas[i]);
                        //var delta = 1 / deltas[i];
                        //if (delta > 3) delta = 3f;
                        //if (delta < -3) delta = -3f;
                        //speeds[i] = (inertia * speeds[i]) + (learnFactor * delta);
                        weights[i] -= speeds[i];
                        if (weights[i] < -5) weights[i] = -5f;
                        else if (weights[i] > 5) weights[i] = 5f;
                    }
                }
                avgError /= testCount;

                if (Math.Abs(avgError - lastError) > learnFactor * 2e-7f)
                {
                    lastError = (float)avgError;
                    equalityCheck = 0;
                }
                else equalityCheck += 1;

                if (avgError > 1.03f * minError || equalityCheck > 199)
                {
                    equalityCheck = 0;
                    lastError = minError;
                    inertia *= .5f;
                    learnFactor *= .95f;
                    Console.WriteLine($"Resetting to last run, new learnFactor = {learnFactor}, new inertia = {inertia}");
                    speeds = new float[_net.NumberOfWeights];
                    Array.Copy(minWeights, weights, weights.Length);
                    avgError = minError;
                    progress(new TrainingJob(_net.Description, (float)avgError, desiredError, learnFactor, inertia, maxRuns - runCount));
                }
                else if (avgError < .995 * minError)
                {
                    minError = (float)avgError;
                    Array.Copy(weights, minWeights, minWeights.Length);
                    _net.ReadWeights(weights);
                    progress(new TrainingJob(_net.Description, (float)avgError, desiredError, learnFactor, inertia, maxRuns - runCount));
                }
                else if (runCount % 200 == 0)
                {
                    _net.ReadWeights(weights);
                    progress(new TrainingJob(_net.Description, (float)avgError, desiredError, learnFactor, inertia, maxRuns - runCount));
                }
            }

            Array.Copy(minWeights, weights, weights.Length);
            _net.ReadWeights(weights);
            progress(new TrainingJob(_net.Description, minError, desiredError, learnFactor, inertia, maxRuns - runCount));
            return minError;
        }
    }
}