using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NerualNet.Logic;

namespace NerualNet.Training
{
    public class SimpleTrainer
    {
        public float Train(
            Net net,
            IEnumerable<Tuple<float[], float[]>> tests,
            float desiredError,
            float maxEpochs,
            float learningRate)
        {
            Console.Write("Getting function... ");
            var train = net.GetTrainingFunction();
            Console.WriteLine("done.");

            var weights = new float[net.NumberOfWeights];
            net.FillWeights(weights);

            var deltas = new float[net.NumberOfWeights];
            var testCount = tests.Count();
            var error = 0f;
            var reportedError = 1f;

            int i;
            for (i = 0; i < maxEpochs; i++)
            {
                error = 0f;
                foreach (var test in tests)
                {
                    error += train(test.Item1, test.Item2, weights, deltas);

                    for (var j = 0; j < weights.Length; j++)
                    {
                        weights[j] -= learningRate * deltas[j];
                    }
                }
                error /= testCount;

                if (error < desiredError)
                {
                    net.ReadWeights(weights);
                    return error;
                }

                if (i % 25 == 0 || error < .98 * reportedError)
                {
                    reportedError = error;
                    Console.WriteLine($"{i}, {error}");
                }
            }

            net.ReadWeights(weights);
            Console.WriteLine($"{i}, {error}");
            return error;
        }
    }
}
