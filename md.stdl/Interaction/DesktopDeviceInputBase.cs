using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public abstract class DesktopDeviceInputManager<TDevice>
    {
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
        /// Selected device ID's
        /// </summary>
        protected int[] SelectedDevices = {-1};

        private readonly DeviceType DevType;

        /// <summary>
        /// Select devices with indices. 
        /// </summary>
        /// <param name="indices">Array of indices. Indices below 0 will create a virtual merged device of all.</param>
        /// <remarks>Internal default index is -1. Calling this function is not mandatory if you're satisfied with a single merged device.</remarks>
        public void SelectDevice(params int[] indices)
        {
            SelectedDevices = indices;
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
                Devices = new TDevice[SelectedDevices.Length];
                DeviceNames = new string[SelectedDevices.Length];
                // An index of -1 means to merge all devices into one
                for (int i = 0; i < SelectedDevices.Length; i++)
                {
                    var index = SelectedDevices[i];
                    if (index < 0 || RawDevices.Count == 0)
                    {
                        Devices[i] = CreateMergedDevice(i);
                        DeviceNames[i] = "Merged";
                    }
                    else
                    {
                        var device = RawDevices[index % RawDevices.Count];
                        Devices[i] = CreateDevice(device, i);
                        DeviceNames[i] = device.DeviceName;
                    }
                }
            }
            else
            {
                Devices = new TDevice[1];
                DeviceNames = new [] { "Dummy" };
                Devices[0] = CreateDummy();
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