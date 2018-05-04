using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.RawInput;
using VVVV.Utils.IO;

// this work is a derivative of:
// https://github.com/vvvv/vvvv-sdk/blob/develop/vvvv45/src/nodes/plugins/System/KeyboardNodes.cs
// originally maintained and developed by the VVVV Group and their community
// for copyright info see CREDITS.md

namespace md.stdl.Interaction
{
    /// <summary>
    /// Observable Keyboard RawInput device wrapper manager
    /// </summary>
    /// <remarks>Creates and manages multiple instances of the observable VVVV.Utils.IO.Keyboard which will then provide notifications about keyboard events</remarks>
    public class KeyboardInputManager : DesktopDeviceInputManager<Keyboard>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public KeyboardInputManager() : base(DeviceType.Keyboard) { }

        /// <summary>
        /// Create a keyboard device
        /// </summary>
        /// <param name="deviceInfo">RawInput device info</param>
        /// <param name="index">Index of the device</param>
        /// <returns></returns>
        protected override Keyboard CreateDevice(DeviceInfo deviceInfo, int index)
        {
            var notifications = Observable.FromEventPattern<KeyboardInputEventArgs>(typeof(Device), "KeyboardInput")
                .Where(ep => ep.EventArgs.Device == deviceInfo.Handle)
                .Select(ep => ep.EventArgs.GetCorrectedKeyboardInputEventArgs())
                .Where(args => args != null)
                .SelectMany(args => GenerateKeyNotifications(args, index));
            return new Keyboard(notifications, true);
        }

        /// <summary>
        /// Create a merged Keyboard device combining data from all the keyboards connected
        /// </summary>
        /// <param name="index">Index of the device</param>
        /// <returns></returns>
        protected override Keyboard CreateMergedDevice(int index)
        {
            var notifications = Observable.FromEventPattern<KeyboardInputEventArgs>(typeof(Device), "KeyboardInput")
                .Select(ep => ep.EventArgs.GetCorrectedKeyboardInputEventArgs())
                .Where(args => args != null)
                .SelectMany(args => GenerateKeyNotifications(args, index));
            return new Keyboard(notifications, true);
        }

        /// <summary>
        /// Create a device which does nothing
        /// </summary>
        /// <returns></returns>
        protected override Keyboard CreateDummy()
        {
            return Keyboard.Empty;
        }

        private IEnumerable<KeyNotification> GenerateKeyNotifications(KeyboardInputEventArgs args, int index)
        {
            Devices[index].CapsLock = Control.IsKeyLocked(Keys.CapsLock);
            var key = args.Key;
            switch (args.State)
            {
                case KeyState.KeyDown:
                case KeyState.SystemKeyDown:
                    if (key == Keys.Menu)
                    {
                        // We need to add the CONTROL key in case of right ALT key
                        if ((args.ScanCodeFlags & ScanCodeFlags.E0) > 0)
                            yield return new KeyDownNotification(Keys.ControlKey);
                    }
                    yield return new KeyDownNotification(key);
                    break;
                case KeyState.KeyUp:
                case KeyState.SystemKeyUp:
                    yield return new KeyUpNotification(key);
                    if (key == Keys.Menu)
                    {
                        // We need to add the CONTROL key in case of right ALT key
                        if ((args.ScanCodeFlags & ScanCodeFlags.E0) > 0)
                            yield return new KeyUpNotification(Keys.ControlKey);
                    }
                    break;
                default:
                    break;
            }
            yield break;
        }
    }
}
