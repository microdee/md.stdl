using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using md.stdl.Boolean;

namespace md.stdl.Tests
{
    public class BooleanTests
    {
        [Fact]
        public void SplitJoinTest()
        {
            var src = BitUtils.Split(0xFFFFFFFF);
            Assert.Equal(32, src.Length);
            Assert.Equal(Enumerable.Repeat(true, 32), src);
            var res = BitUtils.Join(src);
            Assert.Equal(0xFFFFFFFF, res);
        }
        [Fact]
        public void OrTest()
        {
            Assert.Equal<uint>(0x0, BitUtils.Or(0x0, 0x0, 0x0));
            Assert.Equal<uint>(0xC0000000, BitUtils.Or(0x80000000, 0x40000000));
            Assert.Equal<uint>(0xFFFFFFFF, BitUtils.Or(Enumerable.Range(0, 32).Select(val => (uint)0x1 << val).ToArray()));
        }
        [Fact]
        public void AndTest()
        {
            Assert.Equal<uint>(0x0, BitUtils.And(0x0, 0x0, 0x0));
            Assert.Equal<uint>(0x80000008, BitUtils.And(0x80080008, 0x80000008));
            Assert.Equal<uint>(0x0, BitUtils.And(Enumerable.Range(0, 32).Select(val => (uint)0x1 << val).ToArray()));
        }
    }
}
