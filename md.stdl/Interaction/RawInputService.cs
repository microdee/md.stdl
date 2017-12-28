﻿using SharpDX.Multimedia;
using SharpDX.RawInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VVVV.Utils.Win32;

// this work is a derivative of:
// https://github.com/vvvv/vvvv-sdk/blob/develop/vvvv45/src/nodes/plugins/System/RawInputService.cs
// originally maintained and developed by the VVVV Group and their community
// for copyright info see credits.md

namespace md.stdl.Interaction
{
    public class RawInputService
    {
        class MessageOnlyWindow : NativeWindow, IDisposable
        {
            public MessageOnlyWindow()
            {
                // Create a message-only window
                // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632599%28v=vs.85%29.aspx#message_only
                var cp = new CreateParams
                {
                    ClassName = "Message",
                    Parent = Const.HWND_MESSAGE
                };
                this.CreateHandle(cp);
            }

            ~MessageOnlyWindow()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                Dispose(true);
            }

            protected void Dispose(bool disposing)
            {
                if (this.Handle != IntPtr.Zero)
                    this.DestroyHandle();
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == (int)WM.INPUT)
                    Device.HandleMessage(m.LParam, m.HWnd);
                else if (m.Msg == (int)WM.INPUT_DEVICE_CHANGE)
                    RawInputService.EnumerateDevices();
                base.WndProc(ref m);
            }
        }

        private static readonly MessageOnlyWindow messageOnlyWindow;
        static RawInputService()
        {
            messageOnlyWindow = new MessageOnlyWindow();
            var deviceFlags = DeviceFlags.InputSink;
            // DeviceNotify is not supported in Win XP (https://msdn.microsoft.com/de-de/library/windows/desktop/ms645565%28v=vs.85%29.aspx)
            if (Environment.OSVersion.IsWinVistaOrHigher())
                deviceFlags |= DeviceFlags.DeviceNotify;
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
