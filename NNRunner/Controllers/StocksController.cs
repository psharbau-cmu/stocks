using Microsoft.AspNetCore.Mvc;
using NNRunner.NeuralNet;
using NNRunner.StockEvents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NNRunner.Controllers
{
    [Route("api/stocks")]
    public class StocksController : Controller
    {
        private readonly IProcessRepository<TrainingJob> _trainingJobRepository;
        private readonly IEventRepository _events;

        public StocksController(IProcessRepository<TrainingJob> trainingJobRepository, IEventRepository events)
        {
            _trainingJobRepository = trainingJobRepository;
            _events = events;
        }

        [HttpGet("training-jobs")]
        public IEnumerable<Guid> GetJobs()
        {
            return _trainingJobRepository.GetIds();
        }
        
        [HttpGet("training-jobs/{id}")]
        public ProcessProgress<TrainingJob> GetTrainingJobs(Guid id)
        {
            return _trainingJobRepository.GetProcessProgress(id);
        }
        
        [HttpPost("training-jobs")]
        public Guid Post(StocksTrainingJobRequest request)
        {
            // build description
            request.HiddenLayerNodeCounts.Add(2);
            var description = SimpleDescriptionBuilder.GetDescription(4, request.HiddenLayerNodeCounts.ToArray());
            foreach (var id in description.Outputs)
            {
                description.Nodes.Single(n => n.NodeId == id).Processor = null;
            }

            // get net
            var net = Net.FromDescription(description);

            // get training events
            var tests = _events.TrainingEvents
                .Select(evt => Tuple.Create(evt.GetInputArray(), evt.GetOutputArray()));

            // create a trainer
            var trainer = new Trainer(tests, net);

            // add to the repository and return the id
            return _trainingJobRepository
                .CreateProcess(progress => trainer
                    .Train(
                        request.InitialLearningRate,
                        request.InitialMomentum,
                        request.DesiredError,
                        request.MaxIterations,
                        progress,
                        true));
        }
    }
}
