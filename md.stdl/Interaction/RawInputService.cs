using SharpDX.Multimedia;
using SharpDX.RawInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using md.stdl.Windows;
using VVVV.Utils.Win32;

// this work is a derivative of:
// https://github.com/vvvv/vvvv-sdk/blob/develop/vvvv45/src/nodes/plugins/System/RawInputService.cs
// originally maintained and developed by the VVVV Group and their community
// for copyright info see credits.md

namespace md.stdl.Interaction
{
    public class RawInputService
    {

        private static readonly MessageOnlyWindow messageOnlyWindow;
        static RawInputService()
        {
            messageOnlyWindow = new MessageOnlyWindow();
            messageOnlyWindow.OnWndProc += (sender, m) =>
            {
                switch (m.Msg)
                {
                    case (int) WM.INPUT:
                        Device.HandleMessage(m.LParam, m.HWnd);
                        break;
                    case (int) WM.INPUT_DEVICE_CHANGE:
                        EnumerateDevices();
                        break;
                }
            };
            const DeviceFlags deviceFlags = DeviceFlags.InputSink | DeviceFlags.DeviceNotify;
            Device.RegisterDevice(UsagePage.Generic, UsageId.GenericKeyboard, deviceFlags, messageOnlyWindow.Handle, RegisterDeviceOptions.NoFiltering);
            Device.RegisterDevice(UsagePage.Generic, UsageId.GenericMouse, deviceFlags, messageOnlyWindow.Handle, RegisterDeviceOptions.NoFiltering);
        }

        ~RawInputService()
        {
            messageOnlyWindow?.Dispose();
        }

        private static void EnumerateDevices()
        {
            DevicesChanged?.Invoke(null, EventArgs.Empty);
        }

        // Summary:
        //     Occurs when an input device was added or removed.
        public static event EventHandler DevicesChanged;
    }
}
