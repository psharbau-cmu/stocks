using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp
{
    public class Net
    {
        public static Net FromDescription(NetDescription description)
        {
            int maxNodeId = 0;
            int maxInputId = 0;
            int nextWeightId = 0;

            var nodes = new Dictionary<int, Node>();

            foreach (var nodeDescription in description.Nodes)
            {
                var node = new Node(nodeDescription.NodeId, nodeDescription.Aggregator, nodeDescription.Processor);
                nodes.Add(node.Id, node);
                if (node.Id > maxNodeId) maxNodeId = node.Id;

                foreach (var input in nodeDescription.Inputs)
                {
                    node.AddInput(input, nextWeightId);
                    nextWeightId += 1;
                    if (input.FromInputVector && input.InputId > maxInputId) maxInputId = input.InputId;
                }
            }

            var outputNodeId = maxNodeId + 1;
            foreach (var nodeId in description.Outputs)
            {
                nodes[nodeId].AddDownstream(outputNodeId);
            }

            foreach (var node in nodes.Values)
            {
                foreach (var inputNodeId in node.InputNodeNodes)
                {
                    nodes[inputNodeId].AddDownstream(node.Id);
                }
            }

            var nodesWithNoDependencies = new HashSet<int>();
            var orderedNodes = new List<Node>();

            var lastCount = nodes.Count + 1;
            while (orderedNodes.Count < nodes.Count && orderedNodes.Count != lastCount)
            {
                lastCount = orderedNodes.Count;
                foreach (var node in nodes.Values)
                {
                    if (nodesWithNoDependencies.Contains(node.Id)
                        || node.InputNodeNodes.Any(inNodeId => !nodesWithNoDependencies.Contains(inNodeId)))
                    {
                        continue;
                    }

                    nodesWithNoDependencies.Add(node.Id);
                    orderedNodes.Add(node);
                }
            }

            if (orderedNodes.Count < nodes.Count) throw new Exception("Circular dependency in nodes");

            return new Net(maxInputId + 1, nextWeightId, orderedNodes, description.Outputs, outputNodeId);
        }

        private readonly int _inputVectorSize;
        private readonly int _numberOfWeights;
        private readonly int[] _outputNodes;
        private readonly int _outputNodeId;
        private readonly IEnumerable<Node> _forwardOrderedNodes;

        private Net(int inputVectorSize, int numberOfWeights, IEnumerable<Node> forwardOrderedNodes, int[] outputNodes, int outputNodeId)
        {
            _inputVectorSize = inputVectorSize;
            _numberOfWeights = numberOfWeights;
            _forwardOrderedNodes = forwardOrderedNodes;
            _outputNodes = outputNodes;
            _outputNodeId = outputNodeId;
        }
    }
}
