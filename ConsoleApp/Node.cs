using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp
{
    internal class Node
    {
        private readonly string _aggregator;
        private readonly string _processor;

        private HashSet<int> _inputNodes = new HashSet<int>();
        private HashSet<int> _downstreamNodes = new HashSet<int>();

        private Dictionary<NodeInputDescription, int> _inputDescriptions = new Dictionary<NodeInputDescription, int>();

        public Node(int id, string aggregator, string processor)
        {
            Id = id;
            _aggregator = aggregator;
            _processor = processor;
        }

        public int Id { get; }
        public IEnumerable<int> InputNodeNodes => _inputNodes;
        public IEnumerable<int> DownstreamNodes => _downstreamNodes;

        public void AddInput(NodeInputDescription inputDescription, int weightIndex)
        {
            if (!inputDescription.FromInputVector) _inputNodes.Add(inputDescription.InputId);
            _inputDescriptions.Add(inputDescription, weightIndex);
        }

        public void AddDownstream(int nodeId)
        {
            _downstreamNodes.Add(nodeId);
        }

        public void InitializeWeights(float[] weights)
        {
            foreach (var kvp in _inputDescriptions)
            {
                weights[kvp.Value] = kvp.Key.Weight;
            }
        }

        public void ReadWeights(float[] weights)
        {
            foreach (var kvp in _inputDescriptions)
            {
                kvp.Key.Weight = weights[kvp.Value];
            }
        }

        public void AddForwardPropCodeRefWeights(StringBuilder builder)
        {
            var mults = _inputDescriptions
                .Select(d => $"({(d.Key.FromInputVector ? "in" : "out")}{d.Key.InputId} * weights[{d.Value}])");

            builder.Append($"var {(!string.IsNullOrEmpty(_processor) ? $"agg{Id}" : $"out{Id}")} = ");
            
            switch (_aggregator)
            {
                case "sum":
                    builder.Append(string.Join("+", mults));
                    builder.AppendLine(";");
                    break;
                default:
                    throw new Exception($"Unknown aggregator {_aggregator}");
            }

            if (string.IsNullOrEmpty(_processor)) return;

            builder.Append($"var out{Id} = ");
            switch (_processor)
            {
                case "sigmoid":
                    builder.AppendLine($"1 / (1 - Math.Pow(Math.E, -1 * agg{Id}));");
                    break;
                default:
                    throw new Exception($"Unknown processor {_processor}");
            }
        }

        public void AddBackPropCode(StringBuilder builder)
        {
            var varName = $"pIn{Id}";

            builder.Append($"var {varName} = ");
            builder.Append(string.Join("+",_downstreamNodes.Select(d => $"pOut{d}for{Id}")));
            builder.AppendLine(";");
            
            if (!string.IsNullOrEmpty(_processor))
            {
                varName = $"pProc{Id}";
                builder.Append($"var {varName} = ");
                switch (_processor)
                {
                    case "sigmoid":
                        builder.AppendLine($"agg{Id} * (1 - agg{Id}) * pIn{Id};");
                        break;
                    case "default":
                        throw new Exception($"Unknown processor {_processor}");
                }
            }

            switch (_aggregator)
            {
                case "sum":
                    foreach (var input in _inputDescriptions)
                    {
                        builder.AppendLine(
                            $"d[{input.Value}] = (float)({varName} * {(input.Key.FromInputVector ? "in" : "out")}{input.Key.InputId});");
                        if (!input.Key.FromInputVector)
                        {
                            builder.AppendLine($"var pOut{Id}for{input.Key.InputId} = {varName} * weights[{input.Value}];");
                        }
                    }
                    break;
                default:
                    throw new Exception($"Unknown aggregator {_aggregator}");
            }
        }
    }
}
