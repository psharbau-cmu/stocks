using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp;

namespace NNRunner.NeuralNet
{
    public class TrainingJob
    {
        public TrainingJob(
            NetDescription net, 
            float avgErr, 
            float targetErr, 
            float learningRate, 
            float momentum,
            int itsLeft)
        {
            Net = net;
            AvgError = avgErr;
            TargetError = targetErr;
            CurrentLearningRate = learningRate;
            CurrentMomentum = momentum;
            IterationsLeft = itsLeft;
        }

        public NetDescription Net { get; }
        public float AvgError { get; }
        public float TargetError { get; }
        public float CurrentLearningRate { get; }
        public float CurrentMomentum { get; }
        public int IterationsLeft { get; }
    }
}
