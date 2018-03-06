using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp
{
    public struct Event
    {
        public string Symbol;
        public int Day;
        public float Open;
        public float CloseDiff;
        public float SpreadOverLow;
        public float Volume;
        public float NextLow;
        public float TwoDayHigh;
        public float ThreeDayOpen;

        public float[] GetInputArray()
        {
            return new[]
            {
                Open / 1500f,
                CloseDiff * 10,
                (SpreadOverLow - 2.5f) / 15f,
                Volume / 1e9f
            };
        }

        public float[] GetOutputArray()
        {
            return new[]
            {
                (NextLow - Open) / Open,
                (TwoDayHigh - Open) / Open
            };
        }
    }
}
