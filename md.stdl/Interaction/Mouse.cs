﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.RawInput;
using VVVV.Utils.IO;

// this work is a derivative of:
// https://github.com/vvvv/vvvv-sdk/blob/develop/vvvv45/src/nodes/plugins/System/MouseNodes.cs
// originally maintained and developed by the VVVV Group and their community
// for copyright info see CREDITS.md

namespace md.stdl.Interaction
{
    /// <inheritdoc />
    /// <summary>
    /// Observable Mouse RawInput device wrapper manager
    /// </summary>
    /// <remarks>Creates and manages multiple instances of the observable VVVV.Utils.IO.Mouse which will then provide notifications about mouse events</remarks>
    public class MouseInputManager : DesktopDeviceInputManager<Mouse>
    {
        /// <summary>
        /// Indicator whether mouse notifications should be generated from OS Cursor position or from the Raw mouse device
        /// </summary>
        /// <remarks>In case of cursor source only a single merged device wrapper is created</remarks>
        public enum DataSource
        {
            /// <summary>
            /// Cursor position on the desktop
            /// </summary>
            Cursor,
            /// <summary>
            /// Data is directly read from the mouse through RawInput
            /// </summary>
            Raw
        }

        private DataSource _dataSource = DataSource.Raw;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MouseInputManager() : base(DeviceType.Mouse)
        {
            SubscribeToDevices();
        }

        /// <summary>
        /// Changing mouse data source is only supported via this function
        /// </summary>
        /// <param name="mode">The desired data source</param>
        public void ChangeDataSource(DataSource mode, bool selectMerged = false)
        {
            _dataSource = mode;
            switch (mode)
            {
                case DataSource.Cursor: SelectMerged = false; break;
                case DataSource.Raw: SelectMerged = selectMerged; break;
                default: break;
            }
            SubscribeToDevices();
        }

        // Little helper classes to keep track of individual mouse positions
        class MouseData
        {
            public Point Position;
        }

        /// <summary>
        /// Subscribe to devices
        /// </summary>
        protected override void SubscribeToDevices()
        {
            if (_dataSource == DataSource.Cursor)
            {
                Devices = new[] { CreateCursorMouse() };
                WrappedDevices = new List<DeviceWrap<Mouse>> { new DeviceWrap<Mouse>
                {
                    Device = Devices[0],
                    Name = "Cursor",
                    IsMerged = true
                }};
                DeviceNames = new[] {WrappedDevices[0].Name};
            }
            else
            {
                base.SubscribeToDevices();
            }
        }

        /// <summary>
        /// Create a mouse device
        /// </summary>
        /// <param name="deviceInfo">RawInput device info</param>
        /// <param name="index">Index of the device</param>
        /// <returns></returns>
        protected override Mouse CreateDevice(DeviceInfo deviceInfo, int index)
        {
            var initialPosition = Control.MousePosition;
            var mouseData = new MouseData() { Position = initialPosition };
            var mouseInput = Observable.FromEventPattern<MouseInputEventArgs>(typeof(Device), "MouseInput");
            var notifications = mouseInput
                .Where(ep => ep.EventArgs.Device == deviceInfo.Handle)
                .SelectMany<EventPattern<MouseInputEventArgs>, MouseNotification>(ep => GenerateMouseNotifications(mouseData, ep.EventArgs));
            return new Mouse(notifications);
        }

        /// <summary>
        /// Create a merged mouse device combining data from all the mice connected
        /// </summary>
        /// <param name="index">Index of the device</param>
        /// <returns></returns>
        protected override Mouse CreateMergedDevice(int index)
        {
            var initialPosition = Control.MousePosition;
            var mouseData = new MouseData() { Position = initialPosition };
            var mouseInput = Observable.FromEventPattern<MouseInputEventArgs>(typeof(Device), "MouseInput");
            var notifications = mouseInput
                .SelectMany(ep => GenerateMouseNotifications(mouseData, ep.EventArgs));
            return new Mouse(notifications);
        }

        /// <summary>
        /// Create a device which does nothing
        /// </summary>
        /// <returns></returns>
        protected override Mouse CreateDummy()
        {
            return Mouse.Empty;
        }

        private Mouse CreateCursorMouse()
        {
            var initialPosition = Control.MousePosition;
            var mouseData = new MouseData() { Position = initialPosition };
            var mouseInput = Observable.FromEventPattern<MouseInputEventArgs>(typeof(Device), "MouseInput");
            var notifications = mouseInput.SelectMany<EventPattern<MouseInputEventArgs>, MouseNotification>(ep => GenerateCursorNotifications(mouseData, ep.EventArgs));
            return new Mouse(notifications);
        }

        private IEnumerable<MouseNotification> GenerateMouseNotifications(MouseData mouseData, MouseInputEventArgs args)
        {
            var virtualScreenSize = SystemInformation.VirtualScreen.Size;
            var position = mouseData.Position;
            switch (args.Mode)
            {
                case MouseMode.MoveAbsolute:
                    // x,y between 0x0000 and 0xffff
                    position = new Point(args.X / virtualScreenSize.Width, args.Y / virtualScreenSize.Height);
                    break;
                case MouseMode.MoveRelative:

                    // this will keep mouse relative coordinates proportional
                    // so a distance traveled on X axis == same distance traveled on Y axis
                    //float asp = Math.Min(virtualScreenSize.Width, virtualScreenSize.Height);
                    //float screenAspW = virtualScreenSize.Width / asp;
                    //float screenAspH = virtualScreenSize.Height / asp;

                    position = new Point(args.X + position.X, args.Y + position.Y);
                    break;
                case MouseMode.VirtualDesktop:
                    position = new Point(args.X, args.Y);
                    break;
                case MouseMode.AttributesChanged:
                    break;
                case MouseMode.MoveNoCoalesce:
                    break;
                default:
                    break;
            }
            if (mouseData.Position != position)
            {
                mouseData.Position = position;
                yield return new MouseMoveNotification(position, virtualScreenSize);
            }
            foreach (var n in GenerateMouseButtonNotifications(args, position, virtualScreenSize))
                yield return n;
        }

        private IEnumerable<MouseNotification> GenerateCursorNotifications(MouseData mouseData, MouseInputEventArgs args)
        {
            var virtualScreenSize = SystemInformation.VirtualScreen.Size;
            var position = Control.MousePosition;
            if (mouseData.Position != position)
            {
                mouseData.Position = position;
                yield return new MouseMoveNotification(position, virtualScreenSize);
            }
            foreach (var n in GenerateMouseButtonNotifications(args, position, virtualScreenSize))
                yield return n;
        }

        private static IEnumerable<MouseNotification> GenerateMouseButtonNotifications(MouseInputEventArgs args, Point position, Size clientArea)
        {
            var buttonFlags = args.ButtonFlags;
            if ((buttonFlags & MouseButtonFlags.LeftButtonDown) > 0)
                yield return new MouseDownNotification(position, clientArea, MouseButtons.Left);
            if ((buttonFlags & MouseButtonFlags.LeftButtonUp) > 0)
                yield return new MouseUpNotification(position, clientArea, MouseButtons.Left);
            if ((buttonFlags & MouseButtonFlags.RightButtonDown) > 0)
                yield return new MouseDownNotification(position, clientArea, MouseButtons.Right);
            if ((buttonFlags & MouseButtonFlags.RightButtonUp) > 0)
                yield return new MouseUpNotification(position, clientArea, MouseButtons.Right);
            if ((buttonFlags & MouseButtonFlags.MiddleButtonDown) > 0)
                yield return new MouseDownNotification(position, clientArea, MouseButtons.Middle);
            if ((buttonFlags & MouseButtonFlags.MiddleButtonUp) > 0)
                yield return new MouseUpNotification(position, clientArea, MouseButtons.Middle);
            if ((buttonFlags & MouseButtonFlags.Button4Down) > 0)
                yield return new MouseDownNotification(position, clientArea, MouseButtons.XButton1);
            if ((buttonFlags & MouseButtonFlags.Button4Up) > 0)
                yield return new MouseUpNotification(position, clientArea, MouseButtons.XButton1);
            if ((buttonFlags & MouseButtonFlags.Button5Down) > 0)
                yield return new MouseDownNotification(position, clientArea, MouseButtons.XButton2);
            if ((buttonFlags & MouseButtonFlags.Button5Up) > 0)
                yield return new MouseUpNotification(position, clientArea, MouseButtons.XButton2);
            if ((buttonFlags & MouseButtonFlags.MouseWheel) > 0)
            {
                yield return new MouseWheelNotification(position, clientArea, args.WheelDelta);
            }
            if ((buttonFlags & MouseButtonFlags.Hwheel) > 0)
            {
                yield return new MouseHorizontalWheelNotification(position, clientArea, args.WheelDelta);
            }
        }
    }
}
