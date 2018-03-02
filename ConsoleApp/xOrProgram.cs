using System;
using System.Diagnostics;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;

namespace ConsoleApp
{
    class xOrProgram
    {
        static void Main(string[] args)
        { 
            NetDescription description = null;

            using (var reader = new StreamReader(File.OpenRead("xOrNet.json")))
            {
                var text = reader.ReadToEnd();
                description = JsonConvert.DeserializeObject<NetDescription>(text);
            }

            var net = Net.FromDescription(description);

            var train = net.GetTrainingFunction();

            

            var tests = new[]
            {
                new[] {new[] {1f, 1f}, new[] {0f}},
                new[] {new[] {1f, 0f}, new[] {1f}},
                new[] {new[] {0f, 1f}, new[] {1f}},
                new[] {new[] {0f, 0f}, new[] {0f}}
            };

            var weights = new float[net.NumberOfWeights];
            net.FillWeights(weights);

            var stopwatch = new Stopwatch();

            stopwatch.Start();
            for (var i = 0; i < 10000; i++)
            {
                foreach (var test in tests)
                {
                    train(test[0], test[1], weights);
                }
            }
            stopwatch.Stop();

            Console.WriteLine($"1000 runs in dynamic: {stopwatch.ElapsedTicks}");

            stopwatch.Reset();
            stopwatch.Start();
            for (var i = 0; i < 10000; i++)
            {
                foreach (var test in tests)
                {
                    GetDeltas(test[0], test[1], weights);
                }
            }
            stopwatch.Stop();

            Console.WriteLine($"1000 runs in coded: {stopwatch.ElapsedTicks}");

            Console.ReadKey();
        }

        public static float[] GetDeltas(float[] inputs, float[] outputs, float[] weights)
        {
            var d = new float[7];
            var in0 = inputs[0];
            var in1 = inputs[1];
            var agg0 = (in0 * weights[0]) + (in1 * weights[1]);
            var out0 = 1 / (1 - Math.Pow(Math.E, -1 * agg0));
            var agg1 = (in0 * weights[2]) + (in1 * weights[3]);
            var out1 = 1 / (1 - Math.Pow(Math.E, -1 * agg1));
            var agg2 = (out0 * weights[4]) + (out1 * weights[5]);
            var out2 = 1 / (1 - Math.Pow(Math.E, -1 * agg2));
            var error = (float)Math.Sqrt(Math.Pow(out2 - outputs[0], 2));
            d[6] = error;
            var pOut3for2 = (out2 - outputs[0]) / error;
            var pIn2 = pOut3for2;
            var pProc2 = agg2 * (1 - agg2) * pIn2;
            d[4] = (float)(pProc2 * out0);
            var pOut2for0 = pProc2 * weights[4];
            d[5] = (float)(pProc2 * out1);
            var pOut2for1 = pProc2 * weights[5];
            var pIn1 = pOut2for1;
            var pProc1 = agg1 * (1 - agg1) * pIn1;
            d[2] = (float)(pProc1 * in0);
            d[3] = (float)(pProc1 * in1);
            var pIn0 = pOut2for0;
            var pProc0 = agg0 * (1 - agg0) * pIn0;
            d[0] = (float)(pProc0 * in0);
            d[1] = (float)(pProc0 * in1);
            return d;
        }
    }
}
