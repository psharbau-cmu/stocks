using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NerualNet.Logic;

namespace NeuralNet.Builder
{
    public static class SimpleDescriptionBuilder
    {
        public static NetDescription GetDescription(int inputSize, int[] layerCounts)
        {
            var nodes = new List<Node>();

            var lastLayerIds = new int[inputSize];
            for (var i = 0; i < inputSize; i++) lastLayerIds[i] = i;

            var nodeId = 0;
            for (var i = 0; i < layerCounts.Length; i++)
            {
                var layerIds = new int[layerCounts[i]];
                for (var j = 0; j < layerCounts[i]; j++)
                {
                    var node = new Node(nodeId++, "sum", "softplus", 0f, 0);
                    nodes.Add(node);
                    layerIds[j] = node.Id;

                    foreach (var id in lastLayerIds)
                    {
                        node.AddInput(new NodeInput(
                            fromInputVector: i == 0,
                            inputId: id,
                            weightIndex: 0,
                            weight: 0));
                    }
                }

                lastLayerIds = layerIds;
            }

            return new NetDescription()
            {
                Outputs = lastLayerIds,
                Nodes = nodes.Select(node => node.Description).ToArray()
            };
        }
    }
}
