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

        public int NumberOfWeights => _numberOfWeights;

        public void FillWeights(float[] weights)
        {
            foreach (var node in _forwardOrderedNodes)
            {
                node.InitializeWeights(weights);
            }
        }

        public interface IDeltaProvider
        {
            float[] GetDeltas(float[] inputs, float[] outputs, float[] weights);
        }

        public Func<float[], float[], float[], float[]> GetTrainingFunction()
        {
            var builder = new StringBuilder();

            builder.AppendLine("using System;");
            builder.AppendLine("using ConsoleApp;");
            builder.AppendLine("public class DeltaProvider : Net.IDeltaProvider");
            builder.AppendLine("{");
            builder.AppendLine("public float[] GetDeltas(float[] inputs, float[] outputs, float[] weights)");
            builder.AppendLine("{");
            builder.AppendLine($"var d = new float[{_numberOfWeights + 1}];");

            for (var i = 0; i < _inputVectorSize; i++)
            {
                builder.AppendLine($"var in{i} = inputs[{i}];");
            }

            foreach (var node in _forwardOrderedNodes)
            {
                node.AddForwardPropCodeRefWeights(builder);
            }

            builder.Append("var error = (float)Math.Sqrt(");
            builder.AppendJoin("+", _outputNodes.Select((id, i) => $"Math.Pow(out{id} - outputs[{i}], 2)"));
            builder.AppendLine(");");
            builder.AppendLine($"d[{_numberOfWeights}] = error;");

            for (var i = 0; i < _outputNodes.Length; i++)
            {
                builder.Append($"var pOut{_outputNodeId}for{_outputNodes[i]} = ");
                builder.AppendLine($"(out{_outputNodes[i]} - outputs[{i}]) / error;");
            }

            foreach (var node in _forwardOrderedNodes.Reverse())
            {
                node.AddBackPropCode(builder);
            }

            builder.AppendLine("return d;");
            builder.AppendLine("}");
            builder.AppendLine("}");

            var text = builder.ToString();

            var options = ScriptOptions.Default
                .WithReferences(Assembly.GetCallingAssembly());

            var runner = CSharpScript.Create<IDeltaProvider>(text, options)
                .ContinueWith("(new DeltaProvider())")
                .RunAsync()
                .GetAwaiter()
                .GetResult()
                .ReturnValue as IDeltaProvider;

            return (inputs, outputs, weights) => runner.GetDeltas(inputs, outputs, weights);
        }
    }
}