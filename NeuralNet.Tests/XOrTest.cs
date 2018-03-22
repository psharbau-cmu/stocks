using System;
using System.Linq;
using System.Threading;
using NerualNet.Logic;
using NeuralNet.Builder;
using NeuralNet.Training;
using NUnit.Framework;

namespace NeuralNet.Tests
{
    public class XOrTest
    {
        [Test]
        public void NetCanBeTrainedOnXOr()
        {
            var tests = new[]
            {
                Tuple.Create(new[] {1f, -1f}, new[] {1f}),
                Tuple.Create(new[] {1f, 1f}, new[] {0f}),
                Tuple.Create(new[] {-1f, 1f}, new[] {1f}),
                Tuple.Create(new[] {-1f, -1f}, new[] {0f})
            };

            var initialDescription = new NetDescription
            {
                Nodes = new []
                {
                    new NodeDescription
                    {
                        NodeId = 0, Aggregator = "sum", Processor = "sigmoid",
                        Weight = 0.0001f, Inputs = new []
                        {
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 0, Weight = -0.0001f
                            }, 
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 1, Weight = 0.0002f
                            }
                        }
                    },
                    new NodeDescription
                    {
                        NodeId = 1, Aggregator = "sum", Processor = "sigmoid",
                        Weight = -0.0001f, Inputs = new []
                        {
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 0, Weight = 0.0001f
                            },
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 1, Weight = -0.0002f
                            }
                        }
                    },
                    new NodeDescription
                    {
                        NodeId = 2, Aggregator = "sum", Processor = "sigmoid",
                        Weight = -0.0002f, Inputs = new []
                        {
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 0, Weight = -0.0002f
                            },
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 1, Weight = 0.0001f
                            }
                        }
                    },
                    new NodeDescription
                    {
                        NodeId = 3, Aggregator = "sum", Processor = "sigmoid",
                        Weight = 0.0002f, Inputs = new []
                        {
                            new NodeInputDescription
                            {
                                FromInputVector = false, InputId = 0, Weight = -0.0001f
                            },
                            new NodeInputDescription
                            {
                                FromInputVector = false, InputId = 1, Weight = 0.0002f
                            },
                            new NodeInputDescription
                            {
                                FromInputVector = false, InputId = 2, Weight = 0.0002f
                            }
                        }
                    },
                    new NodeDescription
                    {
                        NodeId = 4, Aggregator = "sum", Processor = "sigmoid",
                        Weight = -0.0002f, Inputs = new []
                        {
                            new NodeInputDescription
                            {
                                FromInputVector = false, InputId = 0, Weight = 0.0001f
                            },
                            new NodeInputDescription
                            {
                                FromInputVector = false, InputId = 1, Weight = -0.0002f
                            },
                            new NodeInputDescription
                            {
                                FromInputVector = false, InputId = 2, Weight = -0.0002f
                            }
                        }
                    },
                    new NodeDescription
                    {
                        NodeId = 5, Aggregator = "sum", Processor = "sigmoid",
                        Weight = 0.0002f, Inputs = new []
                        {
                            new NodeInputDescription
                            {
                                FromInputVector = false, InputId = 3, Weight = -0.0001f
                            },
                            new NodeInputDescription
                            {
                                FromInputVector = false, InputId = 4, Weight = 0.0002f
                            }
                        }
                    }
                },
                Outputs = new []{5}
            };

            var net = Net.FromDescription(initialDescription);
            var trainer = new Trainer(tests, net);

            var source = new CancellationTokenSource();
            var token = source.Token;

            var error = trainer.Train(
                learnFactor: .1f,
                inertia: .02f,
                desiredError: 0.1f,
                maxRuns: 20000,
                progress:(j) => { },
                cancel: token);

            Console.WriteLine(error);
            Assert.IsTrue(error < 0.1f);
        }

        [Test]
        public void NetCanBeTrainedOnXOrSoftplus()
        {
            var tests = new[]
            {
                Tuple.Create(new[] {1f, 2f}, new[] {1f}),
                Tuple.Create(new[] {1f, 1f}, new[] {0f}),
                Tuple.Create(new[] {2f, 1f}, new[] {1f}),
                Tuple.Create(new[] {2f, 2f}, new[] {0f})
            };

            //var initialDescription = SimpleDescriptionBuilder.GetDescription(2, new[] {2, 1});
            //foreach (var node in initialDescription.Nodes)
            //{
            //    node.Processor = "sigmoid";
            //}

            //var net = Net.FromDescription(initialDescription);
            //WeightFiller.FillWeights(net, .21f);
            //net.FillWeights(weights);

            var ws = new[]
            {
                .01f, .02f, -.3f,
                -.02f, -.1f, -.03f,
                .03f, -.01f, -.2f
            };
            var deltas = new float[10];
            
            //var train = net.GetTrainingFunction();
            Action<float[], float[],float[],float[]> train = 
                (inputs, outputs, weights, d) =>
                {
                    var in0 = inputs[0];
                    var in1 = inputs[1];
                    var agg0 = (in0 * weights[1]) + (in1 * weights[2]) + weights[0];
                    var out0 = 1 / (1 + Math.Exp(-1 * agg0));
                    var agg1 = (in0 * weights[4]) + (in1 * weights[5]) + weights[3];
                    var out1 = 1 / (1 + Math.Exp(-1 * agg1));
                    var agg2 = (out0 * weights[7]) + (out1 * weights[8]) + weights[6];
                    var out2 = 1 / (1 + Math.Exp(-1 * agg2));
                    var error = (float) (Math.Pow(out2 - outputs[0], 2));
                    d[9] = error;
                    var pOut3for2 = out2 - outputs[0];
                    var pIn2 = pOut3for2;
                    var pProc2 = out2 * (1 - out2) * pIn2;
                    d[6] = (float) pProc2;
                    d[7] = (float) (pProc2 * out0);
                    var pOut2for0 = pProc2 * weights[7];
                    d[8] = (float) (pProc2 * out1);
                    var pOut2for1 = pProc2 * weights[8];
                    var pIn1 = pOut2for1;
                    var pProc1 = out1 * (1 - out1) * pIn1;
                    d[3] = (float) pProc1;
                    d[4] = (float) (pProc1 * in0);
                    d[5] = (float) (pProc1 * in1);
                    var pIn0 = pOut2for0;
                    var pProc0 = out0 * (1 - out0) * pIn0;
                    d[0] = (float) pProc0;
                    d[1] = (float) (pProc0 * in0);
                    d[2] = (float) (pProc0 * in1);
                };

            
            for (var i = 0; i < 50000000; i++)
            {
                var error = 0f;
                foreach (var test in tests)
                {
                    train(test.Item1, test.Item2, ws, deltas);
                    
                    if (deltas.Any(x => float.IsNaN(x) || float.IsInfinity(x) || Math.Abs(x) > 5f))
                    {
                        for (var j = 0; j < ws.Length; j++)
                        {
                            Console.WriteLine($"{ws[j]}, {deltas[j]}");
                        }
                        return;
                    }

                    error += deltas[9];

                    for (var j = 0; j < ws.Length; j++)
                    {
                        ws[j] -= 0.5f * deltas[j];
                    }
                }
                if (i % 50000 == 0) Console.WriteLine($"{i}, {error / 4}");
            }
        }
    }
}