using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using SharpDX.RawInput;
using VVVV.Utils.VMath;
using VVVV.Utils.Win32;

// this work is a derivative of several components found here:
// https://github.com/vvvv/vvvv-sdk/blob/develop/vvvv45/src/nodes/plugins/System
// originally maintained and developed by the VVVV Group and their community
// for copyright info see credits.md

#pragma warning disable CS1591
namespace md.stdl.Interaction
{
    class DeviceComparer : IComparer<DeviceInfo>
    {
        static List<string> FClassCodeOrdering = new List<string>() { "HID", "ACPI", "USB" };
        public int Compare(DeviceInfo x, DeviceInfo y)
        {
            var xIndex = FClassCodeOrdering.IndexOf(x.GetClassCode());
            var yIndex = FClassCodeOrdering.IndexOf(y.GetClassCode());
            if (xIndex == -1) xIndex = int.MaxValue;
            if (yIndex == -1) yIndex = int.MaxValue;
            if (xIndex != yIndex)
                return xIndex.CompareTo(yIndex);
            else
                return x.Handle.ToInt64().CompareTo(y.Handle.ToInt64());
        }
    }

    public struct DeviceDescription
    {
        /// <summary>
        /// Device Name
        /// </summary>
        public string Name;

        /// <summary>
        /// ACPI class code
        /// </summary>
        public string ClassCode;

        /// <summary>
        /// PNP0303 subclass code
        /// </summary>
        public string SubclassCode;

        /// <summary>
        /// 3&amp;13c0b0c5&amp;0 protocol code
        /// </summary>
        public string ProtocolCode;

        /// <summary>
        /// Full string of description
        /// </summary>
        public string DescriptionString;
    }
    public static class RawInputExtensionMethods
    {
        public static List<Keys> ToKeyCodes(this string value)
        {
            return value.Split(',')
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .Select(s =>
                    {
                        Keys keyCode;
                        if (Enum.TryParse<Keys>(s, true, out keyCode))
                            return keyCode;
                        else
                            return Keys.None;
                    }
                )
                .Where(keyCode => keyCode != Keys.None)
                .ToList();
        }

        public static string GetClassCode(this DeviceInfo deviceInfo)
        {
            var deviceName = deviceInfo.DeviceName;
            var indexOfHash = deviceName.IndexOf('#');
            return deviceName.Substring(4, indexOfHash - 4);
        }

        /// <summary>
        /// Gets the potential VID and PID of a device. It only works on devices which contain their HID info in their name
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <param name="vid"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static bool GetVidPid(this DeviceInfo deviceInfo, out int vid, out int pid)
        {
            vid = pid = -1;
            if (deviceInfo == null) return false;
            var vidmatch = Regex.Match(deviceInfo.DeviceName, @"[#&]VID_(?<vid>[A-F\d]+)");
            var pidmatch = Regex.Match(deviceInfo.DeviceName, @"[#&]PID_(?<pid>[A-F\d]+)");
            if (!vidmatch.Success || !pidmatch.Success) return false;
            var pres = int.TryParse(vidmatch.Groups["vid"].Value, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out vid);
            pres = pres && int.TryParse(pidmatch.Groups["pid"].Value, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out pid);

            return pres;
        }

        /// <summary>
        /// Checks if device's HID info matches input VID and PID
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <param name="vid"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static bool IsVidPid(this DeviceInfo deviceInfo, int vid, int pid)
        {
            var res = deviceInfo.GetVidPid(out var cVid, out var cPid);
            if (!res) return false;
            return cVid == vid && cPid == pid;
        }

        public static DeviceDescription GetDeviceDescription(this DeviceInfo deviceInfo)
        {
            // remove the \??\
            var deviceName = deviceInfo.DeviceName;
            deviceName = deviceName.Substring(4);

            var split = deviceName.Split('#');

            var id_01 = split[0];    // ACPI (Class code)
            var id_02 = split[1];    // PNP0303 (SubClass code)
            var id_03 = split[2];    // 3&13c0b0c5&0 (Protocol code)
            var localMachineKey = Registry.LocalMachine;
            using (var key = localMachineKey.OpenSubKey(string.Format(@"System\CurrentControlSet\Enum\{0}\{1}\{2}", id_01, id_02, id_03)))
                return new DeviceDescription
                {
                    Name = deviceName,
                    ClassCode = id_01,
                    SubclassCode = id_02,
                    ProtocolCode = id_03,
                    DescriptionString = (string)key.GetValue("DeviceDesc") ?? ""
                };
        }

        // Thanks to http://molecularmusings.wordpress.com/2011/09/05/properly-handling-keyboard-input/
        public static KeyboardInputEventArgs GetCorrectedKeyboardInputEventArgs(this KeyboardInputEventArgs args)
        {
            var virtualKey = args.Key;
            var scanCode = args.MakeCode;
            var flags = args.ScanCodeFlags;
            if ((int)virtualKey == 255)
            {
                // discard "fake keys" which are part of an escaped sequence
                return null;
            }
            //else if (virtualKey == Keys.ShiftKey)
            //{
            //    // correct left-hand / right-hand SHIFT
            //    virtualKey = (Keys)User32.MapVirtualKey((uint)scanCode, Const.MAPVK_VSC_TO_VK_EX);
            //}
            else if (virtualKey == Keys.NumLock)
            {
                // correct PAUSE/BREAK and NUM LOCK silliness, and set the extended bit
                scanCode = User32.MapVirtualKey((uint)virtualKey, Const.MAPVK_VSC_TO_VK_EX) | 0x100;
            }

            // e0 and e1 are escape sequences used for certain special keys, such as PRINT and PAUSE/BREAK.
            // see http://www.win.tue.nl/~aeb/linux/kbd/scancodes-1.html
            var isE0 = (flags & ScanCodeFlags.E0) != 0;
            var isE1 = (flags & ScanCodeFlags.E1) != 0;

            if (isE1)
            {
                // for escaped sequences, turn the virtual key into the correct scan code using MapVirtualKey.
                // however, MapVirtualKey is unable to map VK_PAUSE (this is a known bug), hence we map that by hand.
                if (virtualKey == Keys.Pause)
                    scanCode = 0x45;
                else
                    scanCode = User32.MapVirtualKey((uint)virtualKey, Const.MAPVK_VK_TO_VSC);
            }

            //switch (virtualKey)
            //{
            //// right-hand CONTROL and ALT have their e0 bit set
            //case Keys.ControlKey:
            //  if (isE0)
            //    virtualKey = Keys.RControlKey;
            //  else
            //    virtualKey = Keys.LControlKey;
            //  break;

            //case Keys.Menu:
            //  if (isE0)
            //    virtualKey = Keys.RMenu;
            //  else
            //    virtualKey = Keys.LMenu;
            //  break;

            //// NUMPAD ENTER has its e0 bit set
            //case Keys.Enter:
            //  if (isE0)
            //    virtualKey = Keys.Enter;
            //  break;

            //// the standard INSERT, DELETE, HOME, END, PRIOR and NEXT keys will always have their e0 bit set, but the
            //// corresponding keys on the NUMPAD will not.
            //case Keys.Insert:
            //  if (!isE0)
            //      virtualKey = Keys.NumPad0;
            //  break;

            //case Keys.Delete:
            //  if (!isE0)
            //    virtualKey = Keys.Decimal;
            //  break;

            //case Keys.Home:
            //  if (!isE0)
            //    virtualKey = Keys.NumPad7;
            //  break;

            //case Keys.End:
            //  if (!isE0)
            //    virtualKey = Keys.NumPad1;
            //  break;

            //case Keys.Prior:
            //  if (!isE0)
            //    virtualKey = Keys.NumPad9;
            //  break;

            //case Keys.Next:
            //  if (!isE0)
            //    virtualKey = Keys.NumPad3;
            //  break;

            //// the standard arrow keys will always have their e0 bit set, but the
            //// corresponding keys on the NUMPAD will not.
            //case Keys.Left:
            //  if (!isE0)
            //    virtualKey = Keys.NumPad4;
            //  break;

            //case Keys.Right:
            //  if (!isE0)
            //    virtualKey = Keys.NumPad6;
            //  break;

            //case Keys.Up:
            //  if (!isE0)
            //    virtualKey = Keys.NumPad8;
            //  break;

            //case Keys.Down:
            //  if (!isE0)
            //    virtualKey = Keys.NumPad2;
            //  break;

            //// NUMPAD 5 doesn't have its e0 bit set
            //case Keys.Clear:
            //  if (!isE0)
            //    virtualKey = Keys.NumPad5;
            //  break;
            //}
            return new KeyboardInputEventArgs()
            {
                Device = args.Device,
                ExtraInformation = args.ExtraInformation,
                Key = virtualKey,
                MakeCode = scanCode,
                ScanCodeFlags = flags,
                State = args.State
            };
        }
    }

    public static class MouseExtensions
    {
        public static readonly Size ClientArea = new Size(short.MaxValue, short.MaxValue);

        public static Point ToMousePoint(this Vector2D normV)
        {
            var clientArea = new Vector2D(ClientArea.Width - 1, ClientArea.Height - 1);
            var v = VMath.Map(normV, new Vector2D(-1, 1), new Vector2D(1, -1), Vector2D.Zero, clientArea, TMapMode.Float);
            return new Point((int)v.x, (int)v.y);
        }

        public static Vector2D FromMousePoint(this Point point, Size clientArea)
        {
            var position = new Vector2D(point.X, point.Y);
            var ca = new Vector2D(clientArea.Width - 1, clientArea.Height - 1);
            return VMath.Map(position, Vector2D.Zero, ca, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);
        }
    }
}

#pragma warning restore CS1591