using System;
using System.Collections.Generic;
using System.Text;

namespace NerualNet.Logic
{
    internal class NodeInput
    {
        private float _weight;

        public NodeInput(bool fromInputVector, int inputId, int weightIndex, float weight)
        {
            FromInputVector = fromInputVector;
            InputId = inputId;
            WeightIndex = weightIndex;
            _weight = weight;
        }

        public bool FromInputVector { get; }
        public int InputId { get; }
        public int WeightIndex { get; }
        public float Weight => _weight;

        public void FillWeight(float[] weights)
        {
            weights[WeightIndex] = _weight;
        }

        public void ReadWeight(float[] weights)
        {
            _weight = weights[WeightIndex];
        }
        

    }
}
