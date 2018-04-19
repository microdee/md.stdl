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

        private readonly Dictionary<TimeSpan, bool> _triggers = new Dictionary<TimeSpan, bool>();

        public void SetTrigger(params TimeSpan[] triggers)
        {
            _triggers.Clear();
            foreach (var time in triggers)
            {
                if(_triggers.ContainsKey(time)) continue;
                _triggers.Add(time, Elapsed >= time);
            }
        }

        public void ResetTriggers()
        {
            foreach (var time in _triggers.Keys)
            {
                _triggers[time] = false;
            }
        }

        public void Mainloop(float deltatime)
        {
            OnMainLoopBegin?.Invoke(this, EventArgs.Empty);
            
            foreach (var trigger in _triggers.Keys)
            {
                if (Elapsed >= trigger && !_triggers[trigger])
                {
                    OnTriggerPassed?.Invoke(this, EventArgs.Empty);
                    _triggers[trigger] = true;
                }
            }

            OnMainLoopEnd?.Invoke(this, EventArgs.Empty);
        }
    }
}
