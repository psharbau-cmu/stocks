using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuralNet;

namespace NerualNet.Builder
{
    public class LayerBuilder
    {
        public struct LayerSpec
        {
            public LayerSpec(int numberOfNodes, string aggregator, string activator)
            {
                NumberOfNodes = numberOfNodes;
                Aggregator = aggregator;
                Activator = activator;
            }

            public int NumberOfNodes { get; }

            public string Aggregator { get; }

            public string Activator { get; }
        }

        private Random _random = new Random();

        public NetDescription BuildDescription(int inputs, IEnumerable<LayerSpec> layers)
        {
            var nodes = new List<NodeDescription>();
            var outputs = layers.Last().NumberOfNodes;

            var lastIds = new int[inputs];
            for (var i = 0; i < inputs; i++) lastIds[i] = i;
            var fromInput = true;

            int nodeId = 0;
            foreach (var layer in layers)
            {
                var newIds = new List<int>();
                for (var i = 0; i < layer.NumberOfNodes; i++)
                {
                    var newNode = new NodeDescription()
                    {
                        NodeId = nodeId++,
                        Aggregator = layer.Aggregator,
                        Processor = layer.Activator,
                        Inputs = lastIds.Select(id => new NodeInputDescription()
                        {
                            FromInputVector = fromInput,
                            InputId = id,
                            Weight = GetWeight(inputs, outputs)
                        }).ToArray(),
                        Weight = GetWeight(inputs, outputs)
                    };
                    newIds.Add(newNode.NodeId);
                    nodes.Add(newNode);
                }
                lastIds = newIds.ToArray();
                fromInput = false;
            }

            return new NetDescription()
            {
                Nodes = nodes.ToArray(),
                Outputs = lastIds.ToArray()
            };
        }

        // Xaiver weight initialization as described here: https://isaacchanghau.github.io/2017/05/24/Weight-Initialization-in-Artificial-Neural-Networks/
        private float GetWeight(int numberOfIns, int numberOfOuts)
        {
            // from https://stackoverflow.com/a/218600
            double u1 = 1.0 - _random.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - _random.NextDouble();
            double randStdNormal =
                Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            return (float)(2 * randStdNormal / (numberOfIns + numberOfOuts)); //random normal(mean,stdDev^2)
        }
    }
}
