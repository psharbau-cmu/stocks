﻿using System;
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
        private static readonly Random _random = new Random();

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
            
            var minWeights = new float[_net.NumberOfWeights];
            float minError = float.MaxValue;
            float avgError = float.MaxValue;
            int runCount = 0;
            while (runCount < maxRuns && avgError > desiredError && learnFactor > 0.005f)
            {
                runCount += 1;

                avgError = 0;
                foreach (var test in _testData)
                {
                    var deltas = getDeltas(test.Item1, test.Item2, weights);
                    avgError += deltas.Last();

                    for (var i = 0; i < weights.Length; i++)
                    {
                        speeds[i] = (inertia * speeds[i]) + (learnFactor * deltas[i]);
                        weights[i] -= speeds[i];
                        if (weights[i] < -3) weights[i] = -3f;
                        else if (weights[i] > 3) weights[i] = 3f;
                    }
                }
                avgError /= _testData.Count();

                if (runCount % 15 == 0) Console.WriteLine($"After {runCount} runs, error is {avgError}");

                if (avgError > 1.03f * minError)
                {
                    inertia *= .5f;
                    learnFactor *= .9f;
                    Console.WriteLine($"Resetting to last run, new learnFactor = {learnFactor}, new inertia = {inertia}");
                    speeds = new float[_net.NumberOfWeights];
                    Array.Copy(minWeights, weights, weights.Length);
                    avgError = minError;
                }
                else if (avgError < .99 * minError)
                {
                    minError = avgError;
                    Array.Copy(weights, minWeights, minWeights.Length);
                }
            }

            Array.Copy(minWeights, weights, weights.Length);
            _net.ReadWeights(weights);
        }
    }
}
