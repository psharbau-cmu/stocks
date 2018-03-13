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
        private readonly IProcessRepository<EvaluationJob, float> _evalJobRepository;
        private readonly IProcessRepository<TrainingJob, float> _trainingJobRepository;
        private readonly IEventRepository _events;

        public StocksController(IProcessRepository<TrainingJob, float> trainingJobRepository, IEventRepository events, IProcessRepository<EvaluationJob, float> evalJobRepository)
        {
            _evalJobRepository = evalJobRepository;
            _trainingJobRepository = trainingJobRepository;
            _events = events;
        }

        [HttpGet("training-jobs")]
        public IEnumerable<Guid> GetJobs()
        {
            return _trainingJobRepository.GetIds();
        }
        
        [HttpGet("training-jobs/{id}")]
        public ProcessProgress<TrainingJob, float> GetTrainingJobs(Guid id)
        {
            return _trainingJobRepository.GetProcessProgress(id);
        }
        
        [HttpPost("training-jobs")]
        public Guid Post(StocksTrainingJobRequest request)
        {
            // build description
            request.HiddenLayerNodeCounts = request.HiddenLayerNodeCounts ?? new List<int>();
            request.HiddenLayerNodeCounts.Add(1);
            var description = SimpleDescriptionBuilder.GetDescription(4, request.HiddenLayerNodeCounts.ToArray());
            foreach (var id in description.Outputs)
            {
                var outNode = description.Nodes.Single(n => n.NodeId == id);
                outNode.Processor = null;
            }

            // get training events
            var tests = _events.TrainingEvents
                .Select(evt => Tuple.Create(evt.GetInputArray(), evt.GetOutputArray()));

            // get net
            var net = Net.FromDescription(description);

            // initialize weights
            WeightFiller.FillWeights(net, request.WeightVariance);

            // create a trainer
            var trainer = new Trainer(tests, net);

            // add to the repository and return the id
            return _trainingJobRepository
                .CreateProcess((progress, token) =>
                {
                    trainer.Train(
                        learnFactor: request.InitialLearningRate,
                        inertia: request.InitialMomentum,
                        desiredError: request.DesiredError,
                        maxRuns: request.MaxIterations,
                        progress: progress,
                        cancel: token);
                });
        }

        [HttpPost("training-jobs/{id}")]
        public void StopJob(Guid id, StocksTrainingJobStopRequest stopRequest)
        {
            if (!stopRequest.Stop) throw new ArgumentException("Well then why'd you send it?");
            _trainingJobRepository.StopProcess(id);
        }

        [HttpPost("evaluate-jobs")]
        public Guid Evaluate([FromBody]StocksEvaluationJobRequest request)
        {
            var net = Net.FromDescription(request.Net);
            var data = _events.TrainingEvents
                .Select(evt => Tuple.Create(evt.GetInputArray(), evt.GetOutputArray()));

            return _evalJobRepository.CreateProcess((action, token) =>
            {
                var eval = net.GetEvaluationFunction();
                var results = new List<Tuple<float, float>>();
                var avgError = 0d;
                var testCount = 0;
                foreach (var test in data)
                {
                    testCount += 1;
                    var result = eval(test.Item1);
                    avgError += Math.Abs(result[0] - test.Item2[0]);
                    results.Add(Tuple.Create(test.Item2[0], result[0]));
                    if (token.IsCancellationRequested) break;
                }
                action(new EvaluationJob()
                {
                    AvgError = (float) (avgError / testCount),
                    ExpectedActuals = results,
                    Net = net.Description
                });
            });
        }

        [HttpGet("evaluate-jobs")]
        public IEnumerable<Guid> GetEvalJobs()
        {
            return _evalJobRepository.GetIds();
        }

        [HttpGet("evalutate-jobs/{id}")]
        public ProcessProgress<EvaluationJob, float> GetEvalJob(Guid id)
        {
            return _evalJobRepository.GetProcessProgress(id);
        }
    }
}
