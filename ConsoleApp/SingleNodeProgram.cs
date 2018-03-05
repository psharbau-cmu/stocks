using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ConsoleApp
{
    class SingleNodeProgram
    {
        public static void Main(string[] args)
        {
            Experiment();

            var net = Net.FromDescription(new NetDescription()
            {
                Nodes = new[]
                {
                    new NodeDescription()
                    {
                        NodeId = 0,
                        Weight = -1f,
                        Aggregator = "sum",
                        Processor = "sigmoid",
                        Inputs = new[]
                        {
                            new NodeInputDescription()
                            {
                                FromInputVector = true,
                                InputId = 0,
                                Weight = .02f
                            }
                        }
                    }
                },
                Outputs = new[] {0}
            });

            var testCases = new[]
            {
                Tuple.Create(new[] {0f}, new[] {1f}),
                Tuple.Create(new[] {1f}, new[] {1f})
            };

            var trainer = new Trainer(testCases, net);

            trainer.Train(.1f, .8f, 0.0001f, 10000, true);

            Console.WriteLine(JsonConvert.SerializeObject(net.Description, Formatting.Indented));

            var forwardFunc = net.GetEvaluationFunction();

            foreach (var test in testCases)
            {
                Console.WriteLine($"{test.Item1[0]} => {forwardFunc(test.Item1)[0]}");
            }

            Console.ReadKey();
        }

        static void Experiment()
        {
            Func<float[], float[], float[], float[]> train = ((float[] inputs, float[] outputs, float[] weights) =>
            {
                var d = new float[3];
                var in0 = inputs[0];
                var agg0 = (in0 * weights[1]) + weights[0];
                var out0 = 1 / (1 + Math.Pow(Math.E, -1 * agg0));
                var error = (float) (Math.Pow(out0 - outputs[0], 2));
                d[2] = error;
                var pOut1for0 = outputs[0] - out0;
                var pIn0 = pOut1for0;
                var pProc0 = agg0 * (1 - agg0) * pIn0;
                d[0] = (float) pProc0;
                d[1] = (float) (pProc0 * in0);
                return d;
            });

            train(new[] {1f}, new[] {1f}, new[] {-10f, 20f});
            train(new[] {0f}, new[] {1f}, new[] {-10f, 20f});

        }
    }
}
