using System;
using System.Linq;
using System.Threading;
using NerualNet.Logic;
using NerualNet.Training;
using NeuralNet.Builder;
using NeuralNet.Training;
using NUnit.Framework;

namespace NeuralNet.Tests
{
    public class XOrTest
    {
        [Test]
        public void SigmoidNetCanBeTrainedOnXOr()
        {
            var tests = new[]
            {
                Tuple.Create(new[] {1f, 0f}, new[] {1f}),
                Tuple.Create(new[] {1f, 1f}, new[] {0f}),
                Tuple.Create(new[] {0f, 1f}, new[] {1f}),
                Tuple.Create(new[] {0f, 0f}, new[] {0f})
            };

            var initialDescription = new NetDescription
            {
                Nodes = new []
                {
                    new NodeDescription
                    {
                        NodeId = 0, Aggregator = "sum", Processor = "sigmoid",
                        Weight = .21f, Inputs = new []
                        {
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 0, Weight = -.07f
                            }, 
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 1, Weight = -.28f
                            }
                        }
                    },
                    new NodeDescription
                    {
                        NodeId = 1, Aggregator = "sum", Processor = "sigmoid",
                        Weight = -.29f, Inputs = new []
                        {
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 0, Weight = .41f
                            },
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 1, Weight = -.05f
                            }
                        }
                    },
                    new NodeDescription
                    {
                        NodeId = 2, Aggregator = "sum", Processor = "sigmoid",
                        Weight = .11f, Inputs = new []
                        {
                            new NodeInputDescription
                            {
                                FromInputVector = false, InputId = 0, Weight = -.1f
                            },
                            new NodeInputDescription
                            {
                                FromInputVector = false, InputId = 1, Weight = -.21f
                            }
                        }
                    }
                },
                Outputs = new []{2}
            };

            var net = Net.FromDescription(initialDescription);
            var trainer = new SimpleTrainer();

            var error = trainer.Train(
                net: net,
                tests: tests,
                desiredError: .001f,
                maxEpochs: 100000,
                learningRate: .5f);

            Console.WriteLine(error);
            Assert.IsTrue(error < 0.1f);
        }

        [Test]
        public void TanhNetCanBeTrainedOnXOr()
        {
            var tests = new[]
            {
                Tuple.Create(new[] {1f, -1f}, new[] {1f}),
                Tuple.Create(new[] {1f, 1f}, new[] {-1f}),
                Tuple.Create(new[] {-1f, 1f}, new[] {1f}),
                Tuple.Create(new[] {-1f, -1f}, new[] {-1f})
            };

            var initialDescription = new NetDescription
            {
                Nodes = new[]
                {
                    new NodeDescription
                    {
                        NodeId = 0, Aggregator = "sum", Processor = "tanh",
                        Weight = .21f, Inputs = new []
                        {
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 0, Weight = -.07f
                            },
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 1, Weight = -.28f
                            }
                        }
                    },
                    new NodeDescription
                    {
                        NodeId = 1, Aggregator = "sum", Processor = "tanh",
                        Weight = -.29f, Inputs = new []
                        {
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 0, Weight = .41f
                            },
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 1, Weight = -.05f
                            }
                        }
                    },
                    new NodeDescription
                    {
                        NodeId = 2, Aggregator = "sum", Processor = "tanh",
                        Weight = .11f, Inputs = new []
                        {
                            new NodeInputDescription
                            {
                                FromInputVector = false, InputId = 0, Weight = -.1f
                            },
                            new NodeInputDescription
                            {
                                FromInputVector = false, InputId = 1, Weight = -.21f
                            }
                        }
                    }
                },
                Outputs = new[] { 2 }
            };

            var net = Net.FromDescription(initialDescription);
            WeightFiller.FillWeights(net, .05f);
            var trainer = new SimpleTrainer();

            var error = trainer.Train(
                net: net,
                tests: tests,
                desiredError: .001f,
                maxEpochs: 100000,
                learningRate: 5f);

            Console.WriteLine(error);
            Assert.IsTrue(error < 0.1f);
        }
    }
}