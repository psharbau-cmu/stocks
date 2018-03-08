using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NNRunner.Controllers
{
    public class StocksTrainingJobRequest
    {
        public List<int> HiddenLayerNodeCounts { get; set; }
        public int MaxIterations { get; set; }
        public float DesiredError { get; set; }
        public float InitialLearningRate { get; set; }
        public float InitialMomentum { get; set; }
    }
}
