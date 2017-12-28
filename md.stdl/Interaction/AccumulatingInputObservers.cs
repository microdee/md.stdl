using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VVVV.Utils.IO;

namespace md.stdl.Interaction
{
    /// <summary>
    /// Accumulates mouse button clicks over time until reset by the implementer
    /// </summary>
    public class AccumulatingMouseClick
    {
        /// <summary>
        /// The accumulated button
        /// </summary>
        public MouseButtons Button { get; private set; }

        /// <summary>
        /// Number of clicks between resets
        /// </summary>
        public int ClickCount { get; private set; }

        /// <summary>
        /// Is this button currently pressed or have been pressed since the last reset
        /// </summary>
        public bool Pressed { get; private set; }

        /// <summary>
        /// Were there a double click recently
        /// </summary>
        public bool DoubleClick { get; private set; }

        /// <summary>
        /// Stopwatch measuring time between clicks
        /// </summary>
        public Stopwatch TimeSinceClicked { get; set; } = new Stopwatch();

        public AccumulatingMouseClick(MouseButtons button)
        {
            Button = button;
            TimeSinceClicked.Start();
        }

        /// <summary>
        /// Reset accumulation. Should be called in a mainloop kind of cycle
        /// </summary>
        public void Reset()
        {
            ClickCount = 0;
            DoubleClick = false;
        }

        /// <summary>
        /// Should be called by an observer or the event of the button press
        /// </summary>
        public void Press()
        {
            ClickCount++;
            Pressed = true;
        }

        /// <summary>
        /// Should be called by an observer or the event of the button release
        /// </summary>
        public void Release()
        {
            if (ClickCount == 0)
                Pressed = false;
            if (TimeSinceClicked.Elapsed.TotalSeconds < 0.18)
                DoubleClick = true;
            TimeSinceClicked.Restart();
        }
    }

    /// <summary>
    /// Accumulating Mouser observer which preserves events between frames and transforms the data of those events into a meaningful form for a per-frame calculation.
    /// </summary>
    public class AccumulatingMouseObserver : IObserver<MouseNotification>
    {
        private IDisposable unsubscriber;

        /// <summary>
        /// The last mouse event notification since the last reset
        /// </summary>
        public MouseNotification LastNotification { get; set; }

        /// <summary>
        /// A filter function to filter the type of notification written into LastNotification. If return is True then LastNotification will be overwritten, otherwise ignored
        /// </summary>
        public Func<MouseNotification, bool> Filter { get; set; }

        /// <summary>
        /// Mouse movement delta between 2 resets (X axis)
        /// </summary>
        public int AccumulatedXDelta => AccCurrPos.X - AccHoldPos.X;
        /// <summary>
        /// Mouse movement delta between 2 resets (Y axis)
        /// </summary>
        public int AccumulatedYDelta => AccCurrPos.Y - AccHoldPos.Y;
        
        /// <summary>
        /// Vertical wheel delta between 2 resets
        /// </summary>
        public int AccumulatedWheelDelta { get; private set; } = 0;
        /// <summary>
        /// Horizontal wheel delta between 2 resets
        /// </summary>
        public int AccumulatedHorizontalWheelDelta { get; private set; } = 0;

        /// <summary>
        /// Accumulated mouse button clicks
        /// </summary>
        public Dictionary<MouseButtons, AccumulatingMouseClick> MouseClicks { get; private set; }

        private Point AccCurrPos;
        private Point AccHoldPos = new Point(0, 0);

        public AccumulatingMouseObserver()
        {
            MouseClicks = new Dictionary<MouseButtons, AccumulatingMouseClick>
            {
                { MouseButtons.Left, new AccumulatingMouseClick(MouseButtons.Left) },
                { MouseButtons.Right, new AccumulatingMouseClick(MouseButtons.Right) },
                { MouseButtons.Middle, new AccumulatingMouseClick(MouseButtons.Middle) },
                { MouseButtons.XButton1, new AccumulatingMouseClick(MouseButtons.XButton1) },
                { MouseButtons.XButton2, new AccumulatingMouseClick(MouseButtons.XButton2) }
            };
        }

        public void OnNext(MouseNotification value)
        {
            switch (value.Kind)
            {
                case MouseNotificationKind.MouseWheel:
                    var wn = (MouseWheelNotification)value;
                    AccumulatedWheelDelta += wn.WheelDelta;
                    break;
                case MouseNotificationKind.MouseHorizontalWheel:
                    var hwn = (MouseHorizontalWheelNotification)value;
                    AccumulatedHorizontalWheelDelta += hwn.WheelDelta;
                    break;
                case MouseNotificationKind.MouseMove:
                    AccCurrPos = value.Position;
                    break;
                case MouseNotificationKind.MouseDown:
                    var mdn = (MouseButtonNotification)value;
                    MouseClicks[mdn.Buttons].Press();
                    break;
                case MouseNotificationKind.MouseUp:
                    var mun = (MouseButtonNotification)value;
                    MouseClicks[mun.Buttons].Release();
                    break;
            }
            if (Filter == null)
            {
                LastNotification = value;
            }
            else if (Filter(value))
            {
                LastNotification = value;
            }
        }

        /// <summary>
        /// Reset accumulation. Should be called in a mainloop kind of cycle
        /// </summary>
        public void ResetAccumulation()
        {
            AccHoldPos = new Point(AccCurrPos.X, AccCurrPos.Y);
            AccumulatedWheelDelta = 0;
            AccumulatedHorizontalWheelDelta = 0;
            foreach (var button in MouseClicks.Values)
            {
                button.Reset();
            }
        }

        public void OnError(Exception error) { }

        public void OnCompleted() { }

        public void SubscribeTo(IObservable<MouseNotification> mouse)
        {
            unsubscriber = mouse?.Subscribe(this);
        }
        public void Unsubscribe()
        {
            unsubscriber?.Dispose();
        }
    }

    /// <summary>
    /// Accumulates key presses over time until reset by the implementer
    /// </summary>
    public class AccumulatingKeyPress : ICloneable
    {
        public Keys KeyCode { get; set; }

        /// <summary>
        /// Is this key currently pressed or have been pressed since the last reset
        /// </summary>
        public bool Pressed { get; set; }

        /// <summary>
        /// Number of keypresses between resets
        /// </summary>
        public int Count { get; set; }

        public object Clone()
        {
            return new AccumulatingKeyPress()
            {
                KeyCode = KeyCode,
                Pressed = Pressed,
                Count = Count
            };
        }
    }

    /// <summary>
    /// Accumulating Keyboard observer which preserves events between frames and transforms the data of those events into a meaningful form for a per-frame calculation.
    /// </summary>
    public class AccumulatingKeyboardObserver : IObserver<KeyNotification>
    {
        private IDisposable unsubscriber;

        /// <summary>
        /// Accumulated keypresses. Keys haven't been pressed since the creation of this observer are not present in the Dictionary
        /// </summary>
        public Dictionary<Keys, AccumulatingKeyPress> Keypresses { get; } = new Dictionary<Keys, AccumulatingKeyPress>();

        public void OnNext(KeyNotification value)
        {
            switch (value.Kind)
            {
                case KeyNotificationKind.KeyDown:
                    var kdn = (KeyDownNotification)value;
                    if (Keypresses.ContainsKey(kdn.KeyCode))
                    {
                        Keypresses[kdn.KeyCode].Count++;
                        Keypresses[kdn.KeyCode].Pressed = true;
                    }
                    else
                    {
                        Keypresses.Add(kdn.KeyCode, new AccumulatingKeyPress()
                        {
                            KeyCode = kdn.KeyCode,
                            Count = 1,
                            Pressed = true
                        });
                    }
                    break;
                case KeyNotificationKind.KeyUp:
                    var kun = (KeyUpNotification)value;
                    if (Keypresses.ContainsKey(kun.KeyCode))
                    {
                        Keypresses[kun.KeyCode].Pressed = false;
                    }
                    break;
            }
        }

        /// <summary>
        /// Reset accumulation. Should be called in a mainloop kind of cycle
        /// </summary>
        public void ResetAccumulation()
        {
            var removables = (from kvp in Keypresses where !kvp.Value.Pressed select kvp.Key).ToArray();
            for (int i = 0; i < removables.Length; i++)
            {
                if (Keypresses.ContainsKey(removables[i]))
                    Keypresses.Remove(removables[i]);
            }
            foreach (var key in Keypresses.Values)
            {
                key.Count = 0;
            }
        }

        public void OnError(Exception error) { }

        public void OnCompleted() { }

        public void SubscribeTo(IObservable<KeyNotification> keyboard)
        {
            unsubscriber = keyboard?.Subscribe(this);
        }
        public void Unsubscribe()
        {
            unsubscriber?.Dispose();
        }
    }
}
