using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NNRunner.NeuralNet;

namespace NNRunner.StockEvents
{
    public class EvaluationJob
    {
        public float AvgError { get; set; }
        public IEnumerable<Tuple<float, float>> ExpectedActuals { get; set; }
        public NetDescription Net { get; set; }
    }
}
