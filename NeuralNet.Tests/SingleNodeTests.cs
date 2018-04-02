using System;
using System.Collections.Generic;
using System.Text;
using NerualNet.Logic;
using NerualNet.Training;
using NUnit.Framework;

namespace NeuralNet.Tests
{
    public class SingleNodeTests
    {
        [Test]
        public void SigmoidNodeCanFigureOutAnd()
        {
            var description = new NetDescription
            {
                Nodes = new [] {
                    new NodeDescription
                    {
                        NodeId = 0, Weight = 0.001f,
                        Aggregator = "sum", Processor = "sigmoid",
                        Inputs = new []
                        {
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 0, Weight = -0.001f
                            },
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 1, Weight = 0.001f
                            }
                        }
                    }
                },
                Outputs = new [] { 0 }
            };

            var net = Net.FromDescription(description);

            var tests = new[]
            {
                Tuple.Create(new[] {1f, 0f}, new[] {0f}),
                Tuple.Create(new[] {1f, 1f}, new[] {1f}),
                Tuple.Create(new[] {0f, 1f}, new[] {0f}),
                Tuple.Create(new[] {0f, 0f}, new[] {0f})
            };

            var trainer = new SimpleTrainer();
            var error = trainer.Train(
                net: net,
                tests: tests,
                desiredError: 0.01f,
                maxEpochs: 5000,
                learningRate:0.5f);

            Assert.IsTrue(error < 0.01f);
        }

        [Test]
        public void TanhNodeCanFigureOutOr()
        {
            var description = new NetDescription
            {
                Nodes = new[] {
                    new NodeDescription
                    {
                        NodeId = 0, Weight = 0.001f,
                        Aggregator = "sum", Processor = "tanh",
                        Inputs = new []
                        {
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 0, Weight = -0.001f
                            },
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 1, Weight = 0.001f
                            }
                        }
                    }
                },
                Outputs = new[] { 0 }
            };

            var net = Net.FromDescription(description);

            var tests = new[]
            {
                Tuple.Create(new[] {1f, 0f}, new[] {1f}),
                Tuple.Create(new[] {1f, 1f}, new[] {1f}),
                Tuple.Create(new[] {0f, 1f}, new[] {1f}),
                Tuple.Create(new[] {0f, 0f}, new[] {-1f})
            };

            var trainer = new SimpleTrainer();
            var loss = trainer.Train(
                net: net,
                tests: tests,
                desiredError: 0.01f,
                maxEpochs: 50000,
                learningRate: 0.5f);

            Assert.IsTrue(loss < 0.01f);
        }

        [Test]
        public void SoftplusNodeCanFigureOutAnd()
        {
            var description = new NetDescription
            {
                Nodes = new[] {
                    new NodeDescription
                    {
                        NodeId = 0, Weight = 0.001f,
                        Aggregator = "sum", Processor = "softplus",
                        Inputs = new []
                        {
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 0, Weight = -0.001f
                            },
                            new NodeInputDescription
                            {
                                FromInputVector = true, InputId = 1, Weight = 0.001f
                            }
                        }
                    }
                },
                Outputs = new[] { 0 }
            };

            var net = Net.FromDescription(description);

            var tests = new[]
            {
                Tuple.Create(new[] {1f, 0f}, new[] {0f}),
                Tuple.Create(new[] {1f, 1f}, new[] {1f}),
                Tuple.Create(new[] {0f, 1f}, new[] {0f}),
                Tuple.Create(new[] {0f, 0f}, new[] {0f})
            };

            var trainer = new SimpleTrainer();
            var error = trainer.Train(
                net: net,
                tests: tests,
                desiredError: 0.01f,
                maxEpochs: 5000,
                learningRate: 0.5f);

            Assert.IsTrue(error < 0.01f);
        }
    }
}
