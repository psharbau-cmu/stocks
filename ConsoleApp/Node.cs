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
        private readonly int _weightIndex;

        private float _weight;
        private HashSet<int> _inputNodes = new HashSet<int>();
        private HashSet<int> _downstreamNodes = new HashSet<int>();

        private Dictionary<NodeInputDescription, int> _inputDescriptions = new Dictionary<NodeInputDescription, int>();

        public Node(int id, string aggregator, string processor, float weight, int weightIndex)
        {
            Id = id;
            _aggregator = aggregator;
            _processor = processor;
            _weight = weight;
            _weightIndex = weightIndex;
        }

        public int Id { get; }
        public IEnumerable<int> InputNodeNodes => _inputNodes;
        public IEnumerable<int> DownstreamNodes => _downstreamNodes;

        public void AddInput(NodeInputDescription inputDescription, int weightIndex)
        {
            if (!inputDescription.FromInputVector) _inputNodes.Add(inputDescription.InputId);
            _inputDescriptions.Add(inputDescription.Clone() as NodeInputDescription, weightIndex);
        }

        public void AddDownstream(int nodeId)
        {
            _downstreamNodes.Add(nodeId);
        }

        public void FillWeights(float[] weights)
        {
            weights[_weightIndex] = _weight;
            foreach (var kvp in _inputDescriptions)
            {
                weights[kvp.Value] = kvp.Key.Weight;
            }
        }

        public void ReadWeights(float[] weights)
        {
            _weight = weights[_weightIndex];
            foreach (var kvp in _inputDescriptions)
            {
                kvp.Key.Weight = weights[kvp.Value];
            }
        }

        public NodeDescription Description => new NodeDescription()
            {
                NodeId = Id,
                Aggregator = _aggregator,
                Processor = _processor,
                Weight = _weight,
                Inputs = _inputDescriptions
                .Select(input => input.Key.Clone() as NodeInputDescription)
                .ToArray()
            };

        public void AddForwardPropCode(StringBuilder builder)
        {
            var mults = _inputDescriptions
                .Select(d => $"({(d.Key.FromInputVector ? "in" : "out")}{d.Key.InputId} * {d.Key.Weight})")
                .Concat(new[] { $"{_weight}" });

            builder.Append($"var {(!string.IsNullOrEmpty(_processor) ? $"agg{Id}" : $"out{Id}")} = ");

            switch (_aggregator)
            {
                case "sum":
                    builder.AppendJoin("+", mults);
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
                    builder.AppendLine($"1 / (1 + Math.Pow(Math.E, -1 * agg{Id}));");
                    break;
                case "softplus":
                    builder.AppendLine($"Math.Log(1 + Math.Exp(agg{Id}));");
                    break;
                default:
                    throw new Exception($"Unknown processor {_processor}");
            }
        }

        public void AddForwardPropCodeRefWeights(StringBuilder builder)
        {
            var mults = _inputDescriptions
                .Select(d => $"({(d.Key.FromInputVector ? "in" : "out")}{d.Key.InputId} * weights[{d.Value}])")
                .Concat(new [] {$"weights[{_weightIndex}]"});

            builder.Append($"var {(!string.IsNullOrEmpty(_processor) ? $"agg{Id}" : $"out{Id}")} = ");
            
            switch (_aggregator)
            {
                case "sum":
                    builder.AppendJoin("+", mults);
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
                    builder.AppendLine($"1 / (1 + Math.Pow(Math.E, -1 * agg{Id}));");
                    break;
                case "softplus":
                    builder.AppendLine($"Math.Log(1 + Math.Exp(agg{Id}));");
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
                    case "softplus":
                        builder.AppendLine($"1 / (1 + Math.Exp(-1 * agg{Id})) * pIn{Id};");
                        break;
                    case "default":
                        throw new Exception($"Unknown processor {_processor}");
                }
            }

            switch (_aggregator)
            {
                case "sum":
                    builder.AppendLine($"d[{_weightIndex}] = (float){varName};");
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
