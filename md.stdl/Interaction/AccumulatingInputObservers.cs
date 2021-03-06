﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using md.stdl.Interfaces;
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
        public bool Pressed => ClickCount > 0 || _internalPressed;
        private bool _internalPressed;

        /// <summary>
        /// Were there a double click recently
        /// </summary>
        public bool DoubleClick { get; private set; }

        /// <summary>
        /// Were there a button-down event recently
        /// </summary>
        public bool ButtonDown { get; private set; }

        /// <summary>
        /// Were there a button-up / released event recently
        /// </summary>
        public bool ButtonUp { get; private set; }

        /// <summary>
        /// Stopwatch measuring time between clicks
        /// </summary>
        public Stopwatch TimeSinceClicked { get; set; } = new Stopwatch();

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="button">The button this AccumulatingMouseClick is assigned to</param>
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
            DoubleClick = ButtonUp = ButtonDown = false;
        }

        /// <summary>
        /// Should be called by an observer or the event of the button press
        /// </summary>
        public void Press()
        {
            ClickCount++;
            ButtonDown = true;
            _internalPressed = true;
        }

        /// <summary>
        /// Should be called by an observer or the event of the button release
        /// </summary>
        public void Release()
        {
            ButtonUp = true;
            _internalPressed = false;
            if (TimeSinceClicked.Elapsed.TotalSeconds < 0.18)
                DoubleClick = true;
            TimeSinceClicked.Restart();
        }
    }

    /// <inheritdoc cref="IMainlooping"/>
    /// <summary>
    /// Accumulating Mouser observer which preserves events between frames and transforms the data of those events into a meaningful form for a per-frame calculation.
    /// </summary>
    public class AccumulatingMouseObserver : IObserver<MouseNotification>, IMainlooping
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
        /// <summary>
        /// Next
        /// </summary>
        /// <param name="value">Next notification</param>
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

        /// <inheritdoc cref="IMainlooping"/>
        public event EventHandler OnMainLoopBegin;
        /// <inheritdoc cref="IMainlooping"/>
        public event EventHandler OnMainLoopEnd;
        /// <inheritdoc cref="IMainlooping"/>
        public void Mainloop(float deltatime)
        {
            OnMainLoopBegin?.Invoke(this, EventArgs.Empty);
            ResetAccumulation();
            OnMainLoopEnd?.Invoke(this, EventArgs.Empty);
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

        /// <inheritdoc cref="IObservable{T}"/>
        public void OnError(Exception error) { }

        /// <inheritdoc cref="IObservable{T}"/>
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
        /// <summary>
        /// The key this AccumulatingKeyPress is assigned to
        /// </summary>
        public Keys KeyCode { get; set; }

        /// <summary>
        /// Is this key currently pressed or have been pressed since the last reset
        /// </summary>
        public bool Pressed => Count > 0 || _internalPressed;
        private bool _internalPressed;

        /// <summary>
        /// Number of keypresses between resets
        /// </summary>
        public int Count { get; set; }

        public void Press()
        {
            Count++;
            _internalPressed = true;
        }

        /// <summary>
        /// reset keypress count between samples
        /// </summary>
        public void Reset()
        {
            Count = 0;
        }

        /// <summary>
        /// Should be called by an observer or the event of the button release
        /// </summary>
        public void Release()
        {
            _internalPressed = false;
        }

        /// <inheritdoc cref="ICloneable"/>
        public object Clone()
        {
            return new AccumulatingKeyPress()
            {
                KeyCode = KeyCode,
                Count = Count
            };
        }
    }

    /// <summary>
    /// Accumulating Keyboard observer which preserves events between frames and transforms the data of those events into a meaningful form for a per-frame calculation.
    /// </summary>
    public class AccumulatingKeyboardObserver : IObserver<KeyNotification>, IMainlooping
    {
        private IDisposable unsubscriber;

        /// <summary>
        /// Accumulated keypresses. Keys haven't been pressed since the creation of this observer are not present in the Dictionary
        /// </summary>
        public Dictionary<Keys, AccumulatingKeyPress> Keypresses { get; } = new Dictionary<Keys, AccumulatingKeyPress>();

        /// <inheritdoc cref="IObservable{T}"/>
        public void OnNext(KeyNotification value)
        {
            switch (value.Kind)
            {
                case KeyNotificationKind.KeyDown:
                    var kdn = (KeyDownNotification)value;
                    if (Keypresses.ContainsKey(kdn.KeyCode))
                    {
                        Keypresses[kdn.KeyCode].Press();
                    }
                    else
                    {
                        Keypresses.Add(kdn.KeyCode, new AccumulatingKeyPress()
                        {
                            KeyCode = kdn.KeyCode
                        });
                        Keypresses[kdn.KeyCode].Press();
                    }
                    break;
                case KeyNotificationKind.KeyUp:
                    var kun = (KeyUpNotification)value;
                    if (Keypresses.ContainsKey(kun.KeyCode))
                    {
                        Keypresses[kun.KeyCode].Release();
                    }
                    break;
            }
        }

        /// <inheritdoc cref="IMainlooping"/>
        public event EventHandler OnMainLoopBegin;
        /// <inheritdoc cref="IMainlooping"/>
        public event EventHandler OnMainLoopEnd;
        /// <inheritdoc cref="IMainlooping"/>
        public void Mainloop(float deltatime)
        {
            OnMainLoopBegin?.Invoke(this, EventArgs.Empty);
            ResetAccumulation();
            OnMainLoopEnd?.Invoke(this, EventArgs.Empty);
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
                key.Reset();
            }
        }

        /// <inheritdoc cref="IObservable{T}"/>
        public void OnError(Exception error) { }

        /// <inheritdoc cref="IObservable{T}"/>
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
