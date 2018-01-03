using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using md.stdl.Interaction;
using VVVV.Utils.IO;
using Xunit;

namespace md.stdl.Tests
{
    public class MouseTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(-1)]
        public void SingleDeviceSelection(int id)
        {
            var mouse = new MouseInputManager();
            Assert.Raises<DeviceListChangedEventArgs<Mouse>>(h => mouse.DeviceListChanged += h, h => mouse.DeviceListChanged -= h, () => { });
            mouse.SelectDevice(id);
            Assert.NotEmpty(mouse.RawDevices);
            Assert.NotEmpty(mouse.Devices);
            Assert.NotEmpty(mouse.DeviceNames);
            Assert.NotNull(mouse.DeviceNames[0]);
            Assert.NotEqual("Dummy", mouse.DeviceNames[0]);
            Assert.True(mouse.DeviceNames[0].Length > 0);
            Console.WriteLine(mouse.DeviceNames[0]);
        }
    }
}
