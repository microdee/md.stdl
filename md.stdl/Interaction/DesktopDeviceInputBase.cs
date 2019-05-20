using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PowerShell.Commands;
using SharpDX.RawInput;

// this work is a derivative of:
// https://github.com/vvvv/vvvv-sdk/blob/develop/vvvv45/src/nodes/plugins/System/DesktopDeviceInputNode.cs
// originally maintained and developed by the VVVV Group and their community
// for copyright info see credits.md

namespace md.stdl.Interaction
{
    /// <summary>
    /// Eventargs for changing devices
    /// </summary>
    /// <typeparam name="TDevice"></typeparam>
    public class DeviceListChangedEventArgs<TDevice> : EventArgs
    {
        /// <summary>
        /// Old devices before the change
        /// </summary>
        public TDevice[] OldDevices;
        /// <summary>
        /// new / current devices
        /// </summary>
        public TDevice[] NewDevices;
    }

    /// <summary>
    /// Base class for RawInput device management.
    /// </summary>
    /// <typeparam name="TDevice">Type of the device wrapper. This is encouraged to be an observable but it's not mandatory.</typeparam>
    /// <remarks>
    /// This is ugly AF but I'm working from what I've got.
    /// </remarks>
    public abstract class DesktopDeviceInputManager<TDevice>
    {
        /// <summary>
        /// Wrapper for all devices
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public struct DeviceWrap<T> where T : TDevice
        {
            /// <summary>
            /// Actual device for wrapping
            /// </summary>
            public T Device { get; internal set; }

            /// <summary>
            /// Name of the device
            /// </summary>
            public string Name { get; internal set; }

            /// <summary>
            /// Associated device info. Can be null if device is merge or dummy.
            /// </summary>
            public DeviceInfo Info { get; internal set; }

            /// <summary>
            /// True if this is a merged device
            /// </summary>
            public bool IsMerged { get; internal set; }

            /// <summary>
            /// True if this device is created from RawInput
            /// </summary>
            public bool IsRaw { get; internal set; }

            /// <summary>
            /// True if this is a dummy device not doing anything.
            /// </summary>
            public bool IsDummy { get; internal set; }
        }

        /// <summary>
        /// Devices wrapped in a convenient class. Like it should have been done in the first place...
        /// </summary>
        public List<DeviceWrap<TDevice>> WrappedDevices { get; protected set; } = new List<DeviceWrap<TDevice>>();

        /// <summary>
        /// Array of selected device wrappers
        /// </summary>
        public TDevice[] Devices { get; protected set; }

        /// <summary>
        /// Name of the selected devices
        /// </summary>
        public string[] DeviceNames { get; protected set; }

        /// <summary>
        /// List of all available RawInput devices for the selected type
        /// </summary>
        public List<DeviceInfo> RawDevices { get; protected set; }

        /// <summary>
        /// Invoked when selected devices are changed.
        /// </summary>
        /// <remarks>In this event ideally the implementer should unsubscribe from old devices.</remarks>
        public event EventHandler<DeviceListChangedEventArgs<TDevice>> DeviceListChanged;

        /// <summary>
        /// Device selector predicates
        /// </summary>
        protected Func<DeviceInfo, int, bool>[] SelectedDevices = Array.Empty<Func<DeviceInfo, int, bool>>();

        /// <summary>
        /// Select Merged device too
        /// </summary>
        public bool SelectMerged { get; set; } = true;

        private readonly DeviceType DevType;

        /// <summary>
        /// Select devices with predicate functions. 
        /// </summary>
        /// <param name="predicates">list of predicates to select individual devices</param>
        /// <remarks>Calling this function is not mandatory if you're satisfied with a single merged device.</remarks>
        public void SelectDevice(bool selectmerged, params Func<DeviceInfo, int, bool>[] predicates)
        {
            SelectMerged = selectmerged;
            SelectedDevices = predicates;
            SubscribeToDevices();
        }

        /// <summary>
        /// Select devices with indices. This is kept here for compatibility please use the overload using predicates.
        /// </summary>
        /// <param name="indices">Array of indices. Indices below 0 will create a virtual merged device of all.</param>
        /// <remarks>Calling this function is not mandatory if you're satisfied with a single merged device.</remarks>
        [Obsolete("Please use the overload using predicates")]
        public void SelectDevice(params int[] indices)
        {
            SelectMerged = indices.Any(i => i < 0);
            SelectedDevices = indices
                .Where(i => i >= 0)
                .Select(i => new Func<DeviceInfo, int, bool>((d, ii) => ii == i))
                .ToArray();
            SubscribeToDevices();
        }

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="deviceType">Inheriting classes should provide a RawInput DeviceType</param>
        /// <remarks>Implementer should call the virtual member SubscribeToDevices() after they created the manager.</remarks>
        protected DesktopDeviceInputManager(DeviceType deviceType)
        {
            DevType = deviceType;
            SelectMerged = true;
            RawInputService.DevicesChanged += (e, s) => SubscribeToDevices();
        }

        /// <summary>
        /// Create respective devices and (re)populate Devices array.
        /// </summary>
        protected virtual void SubscribeToDevices()
        {
            var oldDevices = (TDevice[])Devices?.Clone() ?? new TDevice[0];

            // The following function doesn't work properly in Windows XP, returning installed mouses only.
            RawDevices = Device.GetDevices()
                .Where(d => d.DeviceType == DevType)
                .OrderBy(d => d, new DeviceComparer())
                .ToList();
            // So let's not rely on it if we're running on XP.
            if (RawDevices.Count > 0 || !Environment.OSVersion.IsWinVistaOrHigher())
            {
                WrappedDevices.Clear();
                int ii = 0; // input index
                int io = 0; // output index
                if (SelectMerged)
                {
                    WrappedDevices.Add(new DeviceWrap<TDevice>
                    {
                        Device = CreateMergedDevice(0),
                        Name = "Merged",
                        IsMerged = true
                    });
                    io++;
                }
                foreach (var info in RawDevices)
                {
                    if (SelectedDevices.Any(p => p(info, ii)))
                    {
                        WrappedDevices.Add(new DeviceWrap<TDevice>
                        {
                            Device = CreateDevice(info, io),
                            Info = info,
                            Name = info.DeviceName,
                            IsRaw = true
                        });
                        io++;
                    }
                    ii++;
                }
                Devices = WrappedDevices.Select(d => d.Device).ToArray();
                DeviceNames = WrappedDevices.Select(d => d.Name).ToArray();
            }
            else
            {
                WrappedDevices.Clear();
                WrappedDevices.Add(new DeviceWrap<TDevice>
                {
                    Device = CreateDummy(),
                    Name = "Dummy",
                    IsDummy = true
                });
                Devices = new [] { WrappedDevices[0].Device };
                DeviceNames = new[] { WrappedDevices[0].Name };
            }

            DeviceListChanged?.Invoke(this, new DeviceListChangedEventArgs<TDevice>
            {
                OldDevices = oldDevices,
                NewDevices = Devices
            });
        }

        /// <summary>
        /// Inheriting class should create a real device wrapper.
        /// </summary>
        /// <param name="deviceInfo">Current RawInput device info.</param>
        /// <param name="index">Current index provided by implementer</param>
        /// <returns>An instance of the real device wrapper</returns>
        protected abstract TDevice CreateDevice(DeviceInfo deviceInfo, int index);

        /// <summary>
        /// Inheriting class should create a virtual merged device wrapper.
        /// </summary>
        /// <param name="index">Current index provided by implementer</param>
        /// <returns>An instance of the virtual merged device wrapper</returns>
        protected abstract TDevice CreateMergedDevice(int index);

        /// <summary>
        /// Inheriting class should create a virtual dummy device wrapper which in fact doesn't do anything.
        /// </summary>
        /// <returns>An instance of the virtual dummy device wrapper</returns>
        protected abstract TDevice CreateDummy();
    }
}