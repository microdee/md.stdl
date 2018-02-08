using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace md.stdl.Time
{
    public class StopwatchInteractive : Stopwatch
    {
        private TimeSpan _timeOffset = TimeSpan.Zero;

        public new TimeSpan Elapsed => base.Elapsed + _timeOffset;
        public new long ElapsedMilliseconds => base.ElapsedMilliseconds + (long)_timeOffset.TotalMilliseconds;
        public new long ElapsedTicks => base.ElapsedTicks + _timeOffset.Ticks;

        public void SetTime(TimeSpan time)
        {
            _timeOffset = time;
            if (IsRunning)
            {
                Restart();
            }
            else
            {
                Reset();
            }
        }
    }
}
