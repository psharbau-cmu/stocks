using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NNRunner.NeuralNet
{
    public static class WeightFiller
    {
        private static Random _random = new Random();

        public static void FillWeights(Net net, float variance)
        {
            var weights = new float[net.NumberOfWeights];
            for (var i = 0; i < weights.Length; i++)
            {
                // from https://stackoverflow.com/a/218600
                double u1 = 1.0 - _random.NextDouble(); //uniform(0,1] random doubles
                double u2 = 1.0 - _random.NextDouble();
                double randStdNormal =
                    Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                double randNormal = variance * randStdNormal; //random normal(mean,stdDev^2)

                weights[i] = (float)randNormal;
            }

            net.ReadWeights(weights);
        }

        
    }
}
