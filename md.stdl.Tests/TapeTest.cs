using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using md.stdl.Time;
using VVVV.Utils.VMath;
using Xunit;

namespace md.stdl.Tests
{
    public class TapeTests
    {
        [Fact]
        public void SimpleTapeTest()
        {
            var tape = new Tape<double>(2, 4);
            tape.Default = -1.0;
            Assert.Equal(-1.0, tape[1.0]);
            tape[1.0] = 2.0;
            Assert.Equal(2.0, tape[1.5]);
            tape[0.75] = 3.0;
            Assert.Equal(2.5, tape[0.875]);
            tape[2.0] = 4.0;
            Assert.Equal(3.0, tape[1.5]);
            tape[0.6] = 5.0;
            Assert.Equal(5.0, tape[0.5]);

            var longtape = new Tape<double>(2, 1000);
            longtape[0.5] = 1.0;
            longtape[1.5] = 2.0;
            Assert.Equal(1.5, longtape[1.0]);
        }

        public struct BlendStruct : IBlendable<BlendStruct>
        {
            public double A;
            public double B;
            public double C;

            public BlendStruct(double a, double b, double c)
            {
                A = a;
                B = b;
                C = c;
            }

            public BlendStruct Interpolate(BlendStruct a, BlendStruct b, float alpha)
            {
                return new BlendStruct(
                    VMath.Lerp(a.A, b.A, (double)alpha),
                    VMath.Lerp(a.B, b.B, (double)alpha),
                    VMath.Lerp(a.C, b.C, (double)alpha)
                );
            }

            public Func<BlendStruct, BlendStruct> Copier => null;
            public BlendStruct Default => default;

            public bool Equals(BlendStruct other)
            {
                return A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = A.GetHashCode();
                    hashCode = (hashCode * 397) ^ B.GetHashCode();
                    hashCode = (hashCode * 397) ^ C.GetHashCode();
                    return hashCode;
                }
            }
        }

        [Fact]
        public void BlendableTest()
        {
            var tape = new Tape<BlendStruct>(2.0, 100);
            tape[0.5] = new BlendStruct(1, 11, 111);
            tape[1.5] = new BlendStruct(2, 12, 112);
            Assert.Equal(new BlendStruct(1.5, 11.5, 111.5), tape[1.0]);
        }
    }
}
