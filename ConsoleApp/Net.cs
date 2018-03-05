using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;

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
                var node = new Node(nodeDescription.NodeId, nodeDescription.Aggregator, nodeDescription.Processor, nodeDescription.Weight, nextWeightId++);
                nodes.Add(node.Id, node);
                if (node.Id > maxNodeId) maxNodeId = node.Id;

                foreach (var input in nodeDescription.Inputs)
                {
                    node.AddInput(input, nextWeightId++);
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

        public NetDescription Description => new NetDescription()
        {
            Nodes = _forwardOrderedNodes.Select(node => node.Description).ToArray(),
            Outputs = _outputNodes
        };

        public int NumberOfWeights => _numberOfWeights;

        public void FillWeights(float[] weights)
        {
            foreach (var node in _forwardOrderedNodes)
            {
                node.FillWeights(weights);
            }
        }

        public Func<float[], float[], float[], float[]> GetTrainingFunction()
        {
            var builder = new StringBuilder();

            builder.AppendLine("((float[] inputs, float[] outputs, float[] weights) => {");
            builder.AppendLine($"var d = new float[{_numberOfWeights + 1}];");

            for (var i = 0; i < _inputVectorSize; i++)
            {
                builder.AppendLine($"var in{i} = inputs[{i}];");
            }

            foreach (var node in _forwardOrderedNodes)
            {
                node.AddForwardPropCodeRefWeights(builder);
            }

            builder.Append("var error = (float)(");
            builder.AppendJoin("+", _outputNodes.Select((id, i) => $"Math.Pow(out{id} - outputs[{i}], 2)"));
            builder.AppendLine(");");
            builder.AppendLine($"d[{_numberOfWeights}] = error;");

            for (var i = 0; i < _outputNodes.Length; i++)
            {
                builder.Append($"var pOut{_outputNodeId}for{_outputNodes[i]} = ");
                builder.AppendLine($"out{_outputNodes[i]} - outputs[{i}];");
            }

            foreach (var node in _forwardOrderedNodes.Reverse())
            {
                node.AddBackPropCode(builder);
            }

            builder.AppendLine("return d;");
            builder.AppendLine("})");

            var text = builder.ToString();

            var options = ScriptOptions.Default
                .AddImports("System");

            return CSharpScript
                .Create<Func<float[], float[], float[], float[]>>(text, options)
                .RunAsync()
                .GetAwaiter()
                .GetResult()
                .ReturnValue;
        }

        public Func<float[], float[]> GetEvaluationFunction()
        {
            var builder = new StringBuilder();

            builder.AppendLine("((float[] inputs) => {");
            for (var i = 0; i < _inputVectorSize; i++)
            {
                builder.AppendLine($"var in{i} = inputs[{i}];");
            }
            foreach (var node in _forwardOrderedNodes)
            {
                node.AddForwardPropCode(builder);
            }
            builder.Append("return new float[] { ");
            builder.AppendJoin(",", _outputNodes.Select(id => $" (float) out{id}"));
            builder.AppendLine("};");
            builder.AppendLine("})");

            var text = builder.ToString();

            var options = ScriptOptions.Default
                .AddImports("System");

            return CSharpScript
                .Create<Func<float[], float[]>>(text, options)
                .RunAsync()
                .GetAwaiter()
                .GetResult()
                .ReturnValue;
        }

        public void ReadWeights(float[] weights)
        {
            foreach (var node in _forwardOrderedNodes)
            {
                node.ReadWeights(weights);
            }
        }
    }
}