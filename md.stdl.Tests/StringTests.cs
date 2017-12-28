using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using md.stdl.String;
using Xunit;

namespace md.stdl.Tests
{
    public class StringTests
    {
        private const string _multiline = "Lorem ipsum\r\ndolor sit amet\r\nconsectetur\r\nand the rest of this nonsense";

        [Fact]
        public void DiacriticRemoval()
        {
            Assert.Equal("Arvizturo tukorfurogep", "Árvíztűrő tükörfúrógép".RemoveDiacritics());
        }

        [Fact]
        public void TestLineRange()
        {
            Assert.InRange(_multiline.LineRangeFromCharIndex(18).Length, 14, 16);
        }

        [Fact]
        public void TestMultiEdit()
        {
            var res = _multiline.MultiEdit(new[]
            {
                new EditInsert
                {
                    InsertText = "Radical",
                    Length = 5,
                    Position = 0
                },
                new EditInsert
                {
                    InsertText = "Awesome",
                    Length = 5,
                    Position = 13
                },
            });
            var expected = "Radical ipsum\r\nAwesome sit amet\r\nconsectetur\r\nand the rest of this nonsense";
            Assert.Equal(expected, res);
        }
    }
}
