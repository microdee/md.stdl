using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;
using md.stdl.Xaml;
using Xunit;

namespace md.stdl.Tests
{
    public class MyVectorClass
    {
        [TypeConverter(typeof(Vector2Converter))]
        public Vector2 Point { get; set; }

        [TypeConverter(typeof(Vector3Converter))]
        public Vector3 Position { get; set; }

        [TypeConverter(typeof(Vector4Converter))]
        public Vector4 ScreenPos { get; set; }

        [TypeConverter(typeof(QuaternionConverter))]
        public Quaternion Orientation { get; set; }

        [TypeConverter(typeof(Matrix4x4Converter))]
        public Matrix4x4 Transform { get; set; }
    }

    public class XamlNumericsTest
    {
        [Fact]
        public void TestXaml()
        {
            var srcobj = new MyVectorClass
            {
                Point = new Vector2(0.13f, 2),
                Position = new Vector3(0.13f, 2, 145.2f),
                ScreenPos = new Vector4(0.13f, 2, 145.2f, 0.0f),
                Orientation = new Quaternion(0.13f, 2, 145.2f, 1.0f),
                Transform = new Matrix4x4(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16)
            };
            var ser = XamlServices.Save(srcobj);
            var deserobj = XamlServices.Parse(ser) as MyVectorClass;
            Assert.Equal(srcobj.ScreenPos, deserobj?.ScreenPos);
        }
    }
}
