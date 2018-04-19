using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using md.stdl.Interfaces;

namespace md.stdl.Time
{
    /// <inheritdoc cref="Stopwatch" />
    /// <inheritdoc cref="IMainlooping" />
    /// <summary>
    /// A Seekable version of Stopwatch with the capability to fire time based triggers synchronously
    /// </summary>
    public class StopwatchInteractive : Stopwatch, IMainlooping
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

        public event EventHandler OnMainLoopBegin;
        public event EventHandler OnMainLoopEnd;
        public event EventHandler OnTriggerPassed;

        private readonly List<(TimeSpan time, bool passed)> _triggers = new List<(TimeSpan time, bool passed)>();

        public void SetTrigger(params TimeSpan[] triggers)
        {
            _triggers.Clear();
            foreach (var time in triggers)
            {
                _triggers.Add((time, false));
            }
        }

        public void ResetTriggers()
        {
            for (int i = 0; i < _triggers.Count; i++)
            {
                var trig = _triggers[i];
                trig.passed = false;
                _triggers[i] = trig;
            }
        }

        public void Mainloop(float deltatime)
        {
            OnMainLoopBegin?.Invoke(this, EventArgs.Empty);

            for (int i = 0; i < _triggers.Count; i++)
            {
                var trig = _triggers[i];

                if (Elapsed >= trig.time && !trig.passed)
                {
                    OnTriggerPassed?.Invoke(this, EventArgs.Empty);
                    trig.passed = true;
                }
                _triggers[i] = trig;
            }

            OnMainLoopEnd?.Invoke(this, EventArgs.Empty);
        }
    }
}
