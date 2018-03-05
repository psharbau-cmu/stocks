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
            //NetDescription description = null;

            //using (var reader = new StreamReader(File.OpenRead("xOrNet.json")))
            //{
            //    var text = reader.ReadToEnd();
            //    description = JsonConvert.DeserializeObject<NetDescription>(text);
            //}
            var description = SimpleDescriptionBuilder.GetDescription(2, new[] {2, 1});

            var net = Net.FromDescription(description);
            
            var tests = new[]
            {
                Tuple.Create(new[] {1f, 1f}, new[] {0f}),
                Tuple.Create(new[] {1f, 0f}, new[] {1f}),
                Tuple.Create(new[] {0f, 1f}, new[] {1f}),
                Tuple.Create(new[] {0f, 0f}, new[] {0f})
            };

            var trainer = new Trainer(tests, net);

            trainer.Train(2f, 0.9f, 0.0001f, 1000000, true);

            Console.WriteLine(JsonConvert.SerializeObject(net.Description, Formatting.Indented));

            var forwardFunc = net.GetEvaluationFunction();

            foreach (var test in tests)
            {
                Console.WriteLine($"{test.Item1[0]}, {test.Item1[1]} => {forwardFunc(test.Item1)[0]}");
            }

            Console.ReadKey();
        }

        private static void Experiment()
        {
            Func<float[], float[], float[], float[]> train = (inputs, outputs, weights) =>
                {
                    var d = new float[13];
                    var in0 = inputs[0];
                    var in1 = inputs[1];
                    var agg0 = (in0 * weights[1]) + (in1 * weights[2]) + weights[0];
                    var out0 = 1 / (1 + Math.Pow(Math.E, -1 * agg0));
                    var agg1 = (in0 * weights[4]) + (in1 * weights[5]) + weights[3];
                    var out1 = 1 / (1 + Math.Pow(Math.E, -1 * agg1));
                    var agg2 = (out0 * weights[7]) + (out1 * weights[8]) + weights[6];
                    var out2 = 1 / (1 + Math.Pow(Math.E, -1 * agg2));
                    var agg3 = (out0 * weights[10]) + (out1 * weights[11]) + weights[9];
                    var out3 = 1 / (1 + Math.Pow(Math.E, -1 * agg3));
                    var error = (float) (Math.Pow(out2 - outputs[0], 2) + Math.Pow(out3 - outputs[1], 2));
                    d[12] = error;
                    var pOut4for2 = outputs[0] - out2;
                    var pOut4for3 = outputs[1] - out3;
                    var pIn3 = pOut4for3;
                    var pProc3 = agg3 * (1 - agg3) * pIn3;
                    d[9] = (float) pProc3;
                    d[10] = (float) (pProc3 * out0);
                    var pOut3for0 = pProc3 * weights[10];
                    d[11] = (float) (pProc3 * out1);
                    var pOut3for1 = pProc3 * weights[11];
                    var pIn2 = pOut4for2;
                    var pProc2 = agg2 * (1 - agg2) * pIn2;
                    d[6] = (float) pProc2;
                    d[7] = (float) (pProc2 * out0);
                    var pOut2for0 = pProc2 * weights[7];
                    d[8] = (float) (pProc2 * out1);
                    var pOut2for1 = pProc2 * weights[8];
                    var pIn1 = pOut2for1 + pOut3for1;
                    var pProc1 = agg1 * (1 - agg1) * pIn1;
                    d[3] = (float) pProc1;
                    d[4] = (float) (pProc1 * in0);
                    d[5] = (float) (pProc1 * in1);
                    var pIn0 = pOut2for0 + pOut3for0;
                    var pProc0 = agg0 * (1 - agg0) * pIn0;
                    d[0] = (float) pProc0;
                    d[1] = (float) (pProc0 * in0);
                    d[2] = (float) (pProc0 * in1);
                    return d;
                };

            train(new[] {0f, 0f}, new[] {0f}, new[] {3f });

        }

    }
}
