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
            request.HiddenLayerNodeCounts.Add(1);
            var description = SimpleDescriptionBuilder.GetDescription(4, request.HiddenLayerNodeCounts.ToArray());
            foreach (var id in description.Outputs)
            {
                var outNode = description.Nodes.Single(n => n.NodeId == id);
                outNode.Processor = null;
                foreach (var nodeDescription in description.Nodes.Where(n => outNode.Inputs.Any(i => i.InputId == n.NodeId)))
                {
                    nodeDescription.Aggregator = "min";
                }
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
                .CreateProcess((progress, token) => trainer
                    .Train(
                        request.InitialLearningRate,
                        request.InitialMomentum,
                        request.DesiredError,
                        request.MaxIterations,
                        progress,
                        token,
                        true));
        }

        [HttpPost("training-jobs/{id}")]
        public void StopJob(Guid id, StocksTrainingJobStopRequest stopRequest)
        {
            if (!stopRequest.Stop) throw new ArgumentException("Well then why'd you send it?");
            _trainingJobRepository.StopProcess(id);
        }
    }
}
