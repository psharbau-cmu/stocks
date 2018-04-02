using System;
using System.Collections.Generic;
using System.Text;

namespace UnsupThenSup
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

        private float GetDayOfWeek()
        {
            var daystr = Day.ToString();
            var year = int.Parse(daystr.Substring(0, 4));
            var month = int.Parse(daystr.Substring(4, 2));
            var day = int.Parse(daystr.Substring(6, 2));
            var date = new DateTime(year, month, day);
            var ofWeek = date.DayOfWeek;
            return (((int) ofWeek) - 1) / 5f;
        }

        public float[] GetInputArray()
        {
            return new[]
            {
                Open / 1500f,
                CloseDiff * 10,
                (SpreadOverLow - 2.5f) / 15f,
                Volume / 1e9f,
                GetDayOfWeek()
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

        public float[] ConvertOutputToLowAndHigh(float[] output)
        {
            return new[]
            {
                (output[0] * Open) + Open,
                (output[1] * Open) + Open
            };
        }
    }
}
